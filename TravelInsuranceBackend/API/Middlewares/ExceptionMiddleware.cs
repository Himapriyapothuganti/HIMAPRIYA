using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next   = next;
            _logger = logger;
            _env    = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Move forward through the API pipeline natively!
                await _next(context);
            }
            catch (Exception ex)
            {
                // Intercept any globally bubbled unhandled exceptions!
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex, _env);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception, IHostEnvironment env)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode  = (int)HttpStatusCode.InternalServerError;

            // Generate clean standard response payload avoiding raw strings!
            var response = new
            {
                success = false,
                message = env.IsDevelopment() ? exception.Message : "An internal server error occurred.",
                details = env.IsDevelopment() ? exception.StackTrace?.ToString() : null
            };

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);

            return context.Response.WriteAsync(jsonResponse);
        }
    }
}
