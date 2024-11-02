using SqlAccountRestAPI.Lib;
using SqlAccountRestAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders(); 
builder.Logging.AddConsole(); 
builder.Logging.AddFile("Logs/Request-{Date}.txt"); 

builder.WebHost.UseIISIntegration();  // Thêm dòng này

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<SqlComServer>();

// builder.WebHost.UseKestrel(options =>
// {
//     options.ListenAnyIP(5280); // Cổng HTTP
// });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestResponseLoggingMiddleware>();

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

