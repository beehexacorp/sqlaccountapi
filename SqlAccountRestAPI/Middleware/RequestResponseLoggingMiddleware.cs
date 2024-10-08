using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SqlAccountRestAPI.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            // Log request
            await LogRequest(context);

            // Log response
            await LogResponse(context);
        }

        private async Task LogRequest(HttpContext context)
        {
            context.Request.EnableBuffering(); // Allow the request body to be read multiple times

            var request = context.Request;
            var requestBody = "";

            if (request.ContentLength > 0)
            {
                using (var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
                {
                    requestBody = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0; // Reset body stream position for further processing
                }
            }

            var logMessage = new StringBuilder();
            logMessage.AppendLine("HTTP Request Information:");
            logMessage.AppendLine($"Method: {request.Method}");
            logMessage.AppendLine($"Path: {request.Path}");
            logMessage.AppendLine($"Query: {request.QueryString}");
            logMessage.AppendLine($"Body: {requestBody}");

            _logger.LogInformation(logMessage.ToString());
        }

        private async Task LogResponse(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            await _next(context); // Continue processing the request

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            var logMessage = new StringBuilder();
            logMessage.AppendLine("HTTP Response Information:");
            logMessage.AppendLine($"Status Code: {context.Response.StatusCode}");
            logMessage.AppendLine($"Response Body: {responseBody}");

            _logger.LogInformation(logMessage.ToString());

            await responseBodyStream.CopyToAsync(originalBodyStream);
        }
    }
}
