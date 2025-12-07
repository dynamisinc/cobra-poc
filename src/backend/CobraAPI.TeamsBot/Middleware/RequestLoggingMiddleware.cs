using System.Diagnostics;

namespace CobraAPI.TeamsBot.Middleware;

/// <summary>
/// Middleware that logs all incoming HTTP requests with timing and correlation.
/// Adds correlation ID to all requests for distributed tracing.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public const string CorrelationIdHeader = "X-Correlation-ID";

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get or create correlation ID
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
                            ?? Guid.NewGuid().ToString("N");

        // Add to response headers for tracing
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        // Add to logging scope
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["RequestPath"] = context.Request.Path.ToString(),
            ["RequestMethod"] = context.Request.Method
        });

        var stopwatch = Stopwatch.StartNew();
        var requestId = Activity.Current?.Id ?? context.TraceIdentifier;

        // Log request start (debug level for health checks, info for others)
        var isHealthCheck = context.Request.Path.StartsWithSegments("/api/health");
        var logLevel = isHealthCheck ? LogLevel.Debug : LogLevel.Information;

        _logger.Log(logLevel,
            "HTTP {Method} {Path} started. CorrelationId: {CorrelationId}, RequestId: {RequestId}",
            context.Request.Method,
            context.Request.Path,
            correlationId,
            requestId);

        try
        {
            await _next(context);

            stopwatch.Stop();

            // Log request completion
            _logger.Log(logLevel,
                "HTTP {Method} {Path} completed. Status: {StatusCode}, Duration: {Duration}ms, CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                correlationId);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "HTTP {Method} {Path} failed. Duration: {Duration}ms, CorrelationId: {CorrelationId}, Error: {Error}",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds,
                correlationId,
                ex.Message);

            throw;
        }
    }
}

/// <summary>
/// Extension methods for RequestLoggingMiddleware.
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
