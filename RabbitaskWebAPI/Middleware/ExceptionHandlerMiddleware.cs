using RabbitaskWebAPI.DTOs.Common;
using System.Net;

namespace RabbitaskWebAPI.Middleware
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger, IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var isDevelopment = _environment.IsDevelopment();

            // Create error response without exposing sensitive data in production
            var errorData = isDevelopment
                ? new { exception.StackTrace, InnerException = exception.InnerException?.Message }
                : null;

            var response = new ApiResponse<object>
            {
                Success = false,
                Message = isDevelopment
                    ? exception.Message
                    : "An internal server error occurred. Please contact support.",
                Data = errorData
            };

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
