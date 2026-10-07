using AccessoriesStore.Domain.Exceptions;
using AccessoriesStore.Application.Common.Responses;
using System.Text.Json;

namespace AccessoriesStore.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger)
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
                if (ex is NotFoundException ||
                    ex is BadRequestException ||
                    ex is UnauthorizedException)
                {
                    _logger.LogWarning(
                        ex,
                        "A handled exception occurred while processing the request.");
                }
                else
                {
                    _logger.LogError(
                        ex,
                        "An unhandled exception occurred while processing the request.");
                }

                context.Response.StatusCode = ex switch
                {
                    NotFoundException => StatusCodes.Status404NotFound,
                    BadRequestException => StatusCodes.Status400BadRequest,
                    UnauthorizedException => StatusCodes.Status401Unauthorized,
                    _ => StatusCodes.Status500InternalServerError
                };

                var message = ex switch
                {
                    NotFoundException => ex.Message,
                    BadRequestException => ex.Message,
                    UnauthorizedException => ex.Message,
                    _ => "An unexpected error occurred."
                };

                var response = ApiResponse<object>.Failure(message);

                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
