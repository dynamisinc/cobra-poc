using System.Net;

namespace CobraAPI.TeamsBot.Services;

/// <summary>
/// Configuration options for retry behavior.
/// </summary>
public class RetryOptions
{
    /// <summary>
    /// Maximum number of retry attempts. Default is 3.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Initial delay before the first retry. Default is 1 second.
    /// </summary>
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Maximum delay between retries. Default is 30 seconds.
    /// </summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Multiplier for exponential backoff. Default is 2.0.
    /// </summary>
    public double BackoffMultiplier { get; set; } = 2.0;

    /// <summary>
    /// Whether to add jitter to delay times to prevent thundering herd. Default is true.
    /// </summary>
    public bool AddJitter { get; set; } = true;
}

/// <summary>
/// Result of a retry operation.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
public class RetryResult<T>
{
    /// <summary>
    /// Whether the operation ultimately succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// The result value if successful.
    /// </summary>
    public T? Value { get; init; }

    /// <summary>
    /// The last exception encountered if the operation failed.
    /// </summary>
    public Exception? LastException { get; init; }

    /// <summary>
    /// Number of attempts made (1 = succeeded on first try).
    /// </summary>
    public int Attempts { get; init; }

    /// <summary>
    /// Total time spent including delays.
    /// </summary>
    public TimeSpan TotalDuration { get; init; }

    public static RetryResult<T> Succeeded(T value, int attempts, TimeSpan duration) => new()
    {
        Success = true,
        Value = value,
        Attempts = attempts,
        TotalDuration = duration
    };

    public static RetryResult<T> Failed(Exception? exception, int attempts, TimeSpan duration) => new()
    {
        Success = false,
        LastException = exception,
        Attempts = attempts,
        TotalDuration = duration
    };
}

/// <summary>
/// Provides retry logic with exponential backoff for transient failures.
/// Used for Teams API calls and CobraAPI webhook calls.
/// </summary>
public static class RetryPolicy
{
    private static readonly Random _jitterRandom = new();

    /// <summary>
    /// Executes an async operation with retry logic.
    /// </summary>
    /// <typeparam name="T">The type of result.</typeparam>
    /// <param name="operation">The async operation to execute.</param>
    /// <param name="shouldRetry">Function to determine if a result should trigger a retry.</param>
    /// <param name="options">Retry options. If null, uses defaults.</param>
    /// <param name="logger">Optional logger for retry attempts.</param>
    /// <param name="operationName">Name of the operation for logging.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the operation with retry metadata.</returns>
    public static async Task<RetryResult<T>> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        Func<T, bool> shouldRetry,
        RetryOptions? options = null,
        ILogger? logger = null,
        string operationName = "Operation",
        CancellationToken cancellationToken = default)
    {
        options ??= new RetryOptions();
        var startTime = DateTime.UtcNow;
        Exception? lastException = null;
        var attempt = 0;

        while (attempt <= options.MaxRetries)
        {
            attempt++;
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var result = await operation(cancellationToken);

                if (!shouldRetry(result) || attempt > options.MaxRetries)
                {
                    var duration = DateTime.UtcNow - startTime;
                    if (attempt > 1)
                    {
                        logger?.LogInformation(
                            "{Operation} succeeded after {Attempts} attempts in {Duration:F2}s",
                            operationName, attempt, duration.TotalSeconds);
                    }
                    return RetryResult<T>.Succeeded(result, attempt, duration);
                }

                // Result indicates we should retry
                if (attempt <= options.MaxRetries)
                {
                    var delay = CalculateDelay(attempt, options);
                    logger?.LogWarning(
                        "{Operation} returned retry-able result. Attempt {Attempt}/{MaxAttempts}. Retrying in {Delay:F2}s",
                        operationName, attempt, options.MaxRetries + 1, delay.TotalSeconds);
                    await Task.Delay(delay, cancellationToken);
                }
            }
            catch (Exception ex) when (IsTransientException(ex))
            {
                lastException = ex;

                if (attempt <= options.MaxRetries)
                {
                    var delay = CalculateDelay(attempt, options);
                    logger?.LogWarning(
                        ex,
                        "{Operation} failed with transient error. Attempt {Attempt}/{MaxAttempts}. Retrying in {Delay:F2}s",
                        operationName, attempt, options.MaxRetries + 1, delay.TotalSeconds);
                    await Task.Delay(delay, cancellationToken);
                }
                else
                {
                    logger?.LogError(
                        ex,
                        "{Operation} failed after {Attempts} attempts. Giving up.",
                        operationName, attempt);
                }
            }
            catch (Exception ex)
            {
                // Non-transient exception - don't retry
                logger?.LogError(
                    ex,
                    "{Operation} failed with non-transient error on attempt {Attempt}. Not retrying.",
                    operationName, attempt);
                var duration = DateTime.UtcNow - startTime;
                return RetryResult<T>.Failed(ex, attempt, duration);
            }
        }

        var totalDuration = DateTime.UtcNow - startTime;
        return RetryResult<T>.Failed(lastException, attempt, totalDuration);
    }

    /// <summary>
    /// Executes an async operation with retry logic, for operations that return bool success.
    /// </summary>
    public static async Task<RetryResult<bool>> ExecuteAsync(
        Func<CancellationToken, Task<bool>> operation,
        RetryOptions? options = null,
        ILogger? logger = null,
        string operationName = "Operation",
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(
            operation,
            success => !success, // Retry if operation returns false
            options,
            logger,
            operationName,
            cancellationToken);
    }

    /// <summary>
    /// Executes an async HTTP operation with retry logic.
    /// </summary>
    public static async Task<RetryResult<HttpResponseMessage>> ExecuteHttpAsync(
        Func<CancellationToken, Task<HttpResponseMessage>> operation,
        RetryOptions? options = null,
        ILogger? logger = null,
        string operationName = "HTTP Operation",
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(
            operation,
            response => IsTransientHttpStatusCode(response.StatusCode),
            options,
            logger,
            operationName,
            cancellationToken);
    }

    /// <summary>
    /// Calculates the delay for the next retry attempt using exponential backoff with optional jitter.
    /// </summary>
    private static TimeSpan CalculateDelay(int attempt, RetryOptions options)
    {
        // Exponential backoff: initialDelay * (multiplier ^ (attempt - 1))
        var exponentialDelay = options.InitialDelay.TotalMilliseconds *
                               Math.Pow(options.BackoffMultiplier, attempt - 1);

        // Cap at max delay
        var cappedDelay = Math.Min(exponentialDelay, options.MaxDelay.TotalMilliseconds);

        // Add jitter (±25%) to prevent thundering herd
        if (options.AddJitter)
        {
            var jitterRange = cappedDelay * 0.25;
            var jitter = (_jitterRandom.NextDouble() * 2 - 1) * jitterRange;
            cappedDelay += jitter;
        }

        return TimeSpan.FromMilliseconds(Math.Max(0, cappedDelay));
    }

    /// <summary>
    /// Determines if an exception is transient and should trigger a retry.
    /// </summary>
    public static bool IsTransientException(Exception ex)
    {
        return ex switch
        {
            HttpRequestException httpEx => IsTransientHttpException(httpEx),
            TaskCanceledException => false, // Don't retry cancellations
            OperationCanceledException => false,
            TimeoutException => true,
            System.Net.Sockets.SocketException => true,
            IOException => true,
            _ => false
        };
    }

    /// <summary>
    /// Determines if an HTTP exception is transient.
    /// </summary>
    private static bool IsTransientHttpException(HttpRequestException ex)
    {
        // Check if the status code indicates a transient error
        if (ex.StatusCode.HasValue)
        {
            return IsTransientHttpStatusCode(ex.StatusCode.Value);
        }

        // Network-level errors are typically transient
        return true;
    }

    /// <summary>
    /// Determines if an HTTP status code indicates a transient error.
    /// </summary>
    public static bool IsTransientHttpStatusCode(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.RequestTimeout => true,           // 408
            HttpStatusCode.TooManyRequests => true,          // 429 - Rate limited
            HttpStatusCode.InternalServerError => true,      // 500
            HttpStatusCode.BadGateway => true,               // 502
            HttpStatusCode.ServiceUnavailable => true,       // 503
            HttpStatusCode.GatewayTimeout => true,           // 504
            _ => false
        };
    }
}
