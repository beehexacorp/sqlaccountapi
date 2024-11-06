using SqlAccountRestAPI.Core;
using SqlAccountRestAPI.Helpers;
using SqlAccountRestAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddFile("Logs/Request-{Date}.txt");

builder.WebHost.UseIISIntegration();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<SqlAccountingFactory>();
builder.Services.AddSingleton<SqlAccountingORM>();
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestResponseLoggingMiddleware>();

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

