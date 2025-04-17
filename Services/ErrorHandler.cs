using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;

namespace CollegeWebsite.Services
{
    public class ErrorHandler : IErrorBoundaryLogger
    {
        private readonly ILogger<ErrorHandler> _logger;

        public ErrorHandler(ILogger<ErrorHandler> logger)
        {
            _logger = logger;
        }
        public ValueTask LogErrorAsync(Exception exception)
        {
            _logger.LogError(exception, "An unhandled error occurred in a Blazor component: {Message}", exception.Message);
            return ValueTask.CompletedTask;
        }
    }
}