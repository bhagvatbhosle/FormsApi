using FormsApi.Dtos;
using FormsApi.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FormsApi.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            catch (Exception ex)
            {
                await HandleAsync(context, ex);
            }
        }

        private async Task HandleAsync(HttpContext context, Exception ex)
        {
            var (status, title, detail) = ex switch
            {
                FormNotFoundException => (StatusCodes.Status404NotFound, "Not Found", ex.Message),
                FormConcurrencyException => (StatusCodes.Status409Conflict, "Conflict", ex.Message),
                DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Conflict",
                    "The record was modified by another request. Reload and retry."),

                DbUpdateException => (StatusCodes.Status503ServiceUnavailable, "Service Unavailable",
                    "A database error occurred while processing the request. Please try again later."),

                OperationCanceledException => (499, "Request Cancelled",
                    "The request was cancelled."),

                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error",
                    "An unexpected error occurred. Please try again later.")
            };

            if (status >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
            }
            else
            {
                _logger.LogWarning(ex, "Handled exception ({Status}) processing {Method} {Path}", status, context.Request.Method, context.Request.Path);
            }

            var error = new ApiError(title, status, detail);

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = status;
            await context.Response.WriteAsync(JsonSerializer.Serialize(error));
        }
    }
}
