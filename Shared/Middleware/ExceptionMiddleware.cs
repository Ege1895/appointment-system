using System.Net;
using System.Text.Json;
using AppointmentSystem.Api.Shared.Responses;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AppointmentSystem.Api.Shared.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(context, exception);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception occurred. Type: {ExceptionType}, Message: {Message}", 
                exception.GetType().Name, exception.Message);

            int statusCode = (int)HttpStatusCode.InternalServerError;
            string errorMessage = ErrorMessages.GENERIC_SYSTEM_ERROR;

            if (exception is BaseException baseException)
            {
                statusCode = baseException.StatusCode;
                errorMessage = baseException.Message;
                _logger.LogWarning("Handled application exception: {Message} (Status: {StatusCode})", errorMessage, statusCode);
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            string responseJson = JsonSerializer.Serialize(ApiResponse<object>.FailureResponse(errorMessage), options);
            await context.Response.WriteAsync(responseJson);
        }
    }
}
