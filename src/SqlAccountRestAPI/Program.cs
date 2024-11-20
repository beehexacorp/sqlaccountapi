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

var builder = WebApplication.CreateBuilder(args);
// Log by day
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("log.txt", shared: true, flushToDiskInterval: TimeSpan.FromSeconds(5), rollingInterval: RollingInterval.Hour)
    .CreateLogger();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

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

builder.Services.AddControllers();

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

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.UseExceptionHandler(a => a.Run(async context =>
        {
            var logger = app.Logger;
            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            var exception = exceptionHandlerPathFeature?.Error;
            app.Logger.LogError(exception?.Message ?? exception?.InnerException?.Message, exception?.InnerException ?? exception);
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
    // TODO: login with SQL accounting if there is a cached username & password
});

app.UseAuthorization();

app.MapControllers();
app.Run();

