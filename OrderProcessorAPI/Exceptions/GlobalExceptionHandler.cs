
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace OrderProcessorAPI.Exceptions
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            int statusCode;
            string title;

            switch (exception)
            {
                case ArgumentException:
                    statusCode = StatusCodes.Status400BadRequest;
                    title = "Invalid request";

                    _logger.LogWarning(
                        exception,
                        "Invalid request: {Message}",
                        exception.Message);
                    break;

                case KeyNotFoundException:
                    statusCode = StatusCodes.Status404NotFound;
                    title = "Resource not found";

                    _logger.LogWarning(
                        exception,
                        "Resource not found");
                    break;

                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    title = "Internal server error";

                    _logger.LogError(
                        exception,
                        "An unexpected error occurred");
                    break;
            }

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = statusCode == 500
                    ? "An unexpected error occurred. Please try again."
                    : exception.Message,
                Instance = httpContext.Request.Path
            };

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(
                problem,
                cancellationToken);

            return true;
        }
    }
}
