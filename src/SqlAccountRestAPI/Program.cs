using SqlAccountRestAPI.Core;
using SqlAccountRestAPI.Helpers;
using SqlAccountRestAPI.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;
using MessagePack.Resolvers;
using MessagePack.AspNetCoreMvcFormatter;
using Microsoft.Extensions.FileProviders;


var builder = WebApplication.CreateBuilder(args);
DotNetEnv.Env.Load();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.Services.AddSignalR(); // Add SignalR services

// Get dynamic port from arguments, environment variables, or use a default value
var port = args.Length > 0 ? args[0] :
           Environment.GetEnvironmentVariable("PORT") ??
           builder.Configuration["Kestrel:Port"] ?? "5000";

// Configure Kestrel to use the dynamic port
builder.WebHost.ConfigureKestrel(options =>
{
    Console.WriteLine(@$"Listening to http(s)://localhost:{port}");
    options.ListenLocalhost(int.Parse(port)); // Bind to the specified port
});

builder.WebHost.UseIISIntegration();

builder.Services
    .AddControllers()
    .AddJsonOptions(x =>
    {
        // serialize enums as strings in api responses (e.g. Role)
        x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .AddMvcOptions(option =>
    {
        option.OutputFormatters.Add(new MessagePackOutputFormatter(ContractlessStandardResolver.Options));
        option.InputFormatters.Add(new MessagePackInputFormatter(ContractlessStandardResolver.Options));
    }); ;

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<SqlAccountingFactory>();
builder.Services.AddSingleton<SqlAccountingORM>(provider =>
{
    var sqlAccountingFactory = provider.GetRequiredService<SqlAccountingFactory>();
    return new SqlAccountingLoginHelper(sqlAccountingFactory);
});
builder.Services.AddTransient<SqlAccountingAppHelper>();
builder.Services.AddTransient<SqlAccountingBizObjectHelper>();
builder.Services.AddTransient<SqlAccountingCustomerHelper>();
builder.Services.AddTransient<SqlAccountingStockItemHelper>();
builder.Services.AddTransient<SqlAccountingSalesOrderHelper>();
builder.Services.AddTransient<SqlAccountingSaleInvoiceHelper>();
builder.Services.AddTransient<SqlAccountingCustomerInvoiceHelper>();
builder.Services.AddTransient<SqlAccountingCustomerPaymentHelper>();
builder.Services.AddTransient<SqlAccountingStockAdjustmentHelper>();
builder.Services.AddTransient<SqlAccountingStockItemTemplateHelper>();

if (OperatingSystem.IsWindows())
{
    builder.Host.UseWindowsService();
}

builder.Services.AddCors(options =>
{
    Console.WriteLine($"Is Development {builder.Environment.IsDevelopment()}");
    if (builder.Environment.IsDevelopment())
    {
        var frontendPort = Environment.GetEnvironmentVariable("FRONTEND_PORT") ?? "3000";
        Console.WriteLine($"Accepted Frontend Port {frontendPort}");
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins($"https://localhost:{frontendPort}", $"http://localhost:{frontendPort}")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    }
});

var app = builder.Build();
// Log by day
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("log.txt", shared: true, flushToDiskInterval: TimeSpan.FromSeconds(1), rollingInterval: RollingInterval.Hour)
    .WriteTo.SignalR(app.Services.GetRequiredService<IHubContext<NotificationHub>>())
    .CreateLogger();

// Serve static files under wwwroot/dashboard/dist
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.WebRootPath, "dashboard", "dist")),
    RequestPath = "/dashboard"
});

// Explicitly map /dashboard/favicon.ico to the correct file
app.Map("/dashboard/favicon.ico", appBuilder =>
{
    appBuilder.Run(async context =>
    {
        var filePath = Path.Combine(builder.Environment.WebRootPath, "dashboard", "dist", "favicon.ico");
        if (System.IO.File.Exists(filePath))
        {
            context.Response.ContentType = "image/x-icon";
            await context.Response.SendFileAsync(filePath);
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
        }
    });
});

// Fallback to index.html for SPA routes
var hubBuilder = app.MapHub<NotificationHub>("/notification-hub"); // Endpoint for SignalR hub
if (builder.Environment.IsDevelopment())
{
    hubBuilder.RequireCors("AllowFrontend");
}
app.MapFallbackToFile("/dashboard/{*path:nonfile}", "dashboard/dist/index.html");

app.Use(async (context, next) =>
{
    Console.WriteLine($"Request Path: {context.Request.Path}");
    await next();
});
app.UseCors("AllowFrontend");

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.UseExceptionHandler(a => a.Run(async context =>
        {
            // var logger = app.Logger;
            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            var exception = exceptionHandlerPathFeature?.Error;
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(exception, exception?.Message);
            logger.LogError(exception?.ToString());
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                errorMessage = exception?.InnerException?.Message ?? exception?.Message
            }));
        }));

var applicationLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
applicationLifetime.ApplicationStopped.Register(() =>
{
    var sqlAccountingAppFactory = app.Services.GetRequiredService<SqlAccountingFactory>();
    sqlAccountingAppFactory.Dispose();
});

applicationLifetime.ApplicationStarted.Register(() =>
{
    // Output the port and URLs the application is running on
    var serverAddressesFeature = app.Services.GetService<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>();
    if (serverAddressesFeature != null)
    {
        foreach (var address in serverAddressesFeature.Addresses)
        {
            Console.WriteLine($"Application is running on: {address}");
        }
    }
    // Optional: Log environment
    Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
    Console.WriteLine($"Application started successfully.");

});

app.UseAuthorization();

app.MapControllers();
app.Run();