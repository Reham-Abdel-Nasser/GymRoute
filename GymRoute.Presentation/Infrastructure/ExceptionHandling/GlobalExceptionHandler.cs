using GymRoute.Presentation.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;

namespace GymRoute.Presentation.Infrastructure.ExceptionHandling;

/// <summary>
/// Global exception handler (IExceptionHandler). Logs the error, stores dev details,
/// and redirects the user to the shared Error.cshtml page.
/// </summary>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IMemoryCache cache,
    IHostEnvironment environment) : IExceptionHandler
{
    private const string ErrorCacheKeyPrefix = "error:";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode = exception switch
        {
            GymNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        var requestId = httpContext.TraceIdentifier;

        logger.LogError(
            exception,
            "Unhandled exception. Status={StatusCode} TraceId={TraceId} Method={Method} Path={Path} Query={Query}",
            statusCode,
            requestId,
            httpContext.Request.Method,
            httpContext.Request.Path,
            httpContext.Request.QueryString.Value);

        var errorDetails = new ErrorViewModel
        {
            RequestId = requestId,
            StatusCode = statusCode,
            Title = statusCode == StatusCodes.Status404NotFound
                ? "Not found"
                : "Something went wrong",
            Message = statusCode == StatusCodes.Status404NotFound
                ? exception.Message
                : "An unexpected error occurred. Please try again later.",
            ExceptionType = exception.GetType().Name,
            ShowDetails = environment.IsDevelopment()
        };

        if (environment.IsDevelopment())
        {
            errorDetails.Message = exception.Message;
            cache.Set($"{ErrorCacheKeyPrefix}{requestId}", errorDetails, TimeSpan.FromMinutes(5));
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.Redirect(BuildErrorUrl(httpContext, requestId));

        return true;
    }

    private static string BuildErrorUrl(HttpContext httpContext, string requestId)
    {
        var pathBase = httpContext.Request.PathBase.HasValue
            ? httpContext.Request.PathBase.Value
            : string.Empty;

        return $"{pathBase}/Home/Error?requestId={Uri.EscapeDataString(requestId)}";
    }

    public static ErrorViewModel? GetCachedError(IMemoryCache cache, string? requestId)
    {
        if (string.IsNullOrEmpty(requestId))
            return null;

        return cache.Get<ErrorViewModel>($"{ErrorCacheKeyPrefix}{requestId}");
    }
}
