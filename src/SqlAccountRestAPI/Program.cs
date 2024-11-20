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
using Serilog.Events;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json.Serialization;
using Serilog.Core;
using Microsoft.AspNetCore.SignalR;
using Serilog.Configuration;

var builder = WebApplication.CreateBuilder(args);

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
    options.ListenLocalhost(int.Parse(port)); // Bind to the specified port
});
builder.WebHost.UseIISIntegration();

builder.Services.AddControllers().AddJsonOptions(x =>
{
    // serialize enums as strings in api responses (e.g. Role)
    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
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
builder.Services.AddTransient<SqlAccountingSaleOrderHelper>();
builder.Services.AddTransient<SqlAccountingSaleInvoiceHelper>();
builder.Services.AddTransient<SqlAccountingCustomerInvoiceHelper>();
builder.Services.AddTransient<SqlAccountingCustomerPaymentHelper>();
builder.Services.AddTransient<SqlAccountingStockAdjustmentHelper>();
builder.Services.AddTransient<SqlAccountingStockItemTemplateHelper>();

if (OperatingSystem.IsWindows())
{
    builder.Host.UseWindowsService();
}

var app = builder.Build();
// Log by day
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("log.txt", shared: true, flushToDiskInterval: TimeSpan.FromSeconds(5), rollingInterval: RollingInterval.Hour)
    .WriteTo.SignalR(app.Services.GetRequiredService<IHubContext<NotificationHub>>())
    .CreateLogger();
// Serve static files
app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true, // Serve files with unknown extensions
    DefaultContentType = "application/javascript", // Default MIME type if unknown
    OnPrepareResponse = ctx =>
    {
        // Ensure proper MIME type for .js and .css files
        var fileExtension = Path.GetExtension(ctx.File.Name);
        if (fileExtension == ".js")
        {
            ctx.Context.Response.ContentType = "application/javascript";
        }
        else if (fileExtension == ".css")
        {
            ctx.Context.Response.ContentType = "text/css";
        }
    }
});

// Fallback to index.html for SPA routes
app.MapHub<NotificationHub>("/notification-hub"); // Endpoint for SignalR hub
app.MapFallbackToFile("/dashboard/{*path:nonfile}", "dashboard/dist/index.html");
// app.MapFallback(context =>
// {
//     // Check if the path refers to a file
//     var path = context.Request.Path.Value;
//     if (path != null && (path.EndsWith(".js") || path.EndsWith(".css") || path.EndsWith(".html") || path.EndsWith(".ico")))
//     {
//         return Task.CompletedTask; // Let static files middleware handle it
//     }

//     context.Response.ContentType = "text/html";
//     return context.Response.SendFileAsync(Path.Combine(builder.Environment.WebRootPath, "dashboard", "index.html"));
// });
app.Use(async (context, next) =>
{
    Console.WriteLine($"Request Path: {context.Request.Path}");
    await next();
});

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
    // TODO: login with SQL account if there is a cached username & password
});

app.UseAuthorization();

app.MapControllers();
app.Run();



public class SignalRSink : ILogEventSink, IDisposable
{
    private readonly IFormatProvider _formatProvider;
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRSink(IHubContext<NotificationHub> hubContext, IFormatProvider formatProvider)
    {
        _hubContext = hubContext;
        _formatProvider = formatProvider;
    }

    public void Emit(LogEvent logEvent)
    {
        // Format the log message
        var message = logEvent.RenderMessage(_formatProvider);

        // Send the message to all connected SignalR clients
        Task.Run(() =>
            _hubContext.Clients.All.SendAsync(NotificationHub.RECEIVE_LOG_EVENT, logEvent.Level.ToString(), message, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
        );
    }

    public void Dispose()
    {
        // Cleanup resources if necessary
    }
}

public static class SignalRLoggerConfigurationExtensions
{
    public static LoggerConfiguration SignalR(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        IHubContext<NotificationHub> hubContext,
        IFormatProvider formatProvider = null)
    {
        return loggerSinkConfiguration.Sink(new SignalRSink(hubContext, formatProvider));
    }
}