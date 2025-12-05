using System.Net;
using CobraAPI.TeamsBot.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace CobraAPI.TeamsBot.Tests.Services;

public class RetryPolicyTests
{
    private readonly Mock<ILogger> _loggerMock;

    public RetryPolicyTests()
    {
        _loggerMock = new Mock<ILogger>();
    }

    [Fact]
    public async Task ExecuteAsync_SucceedsOnFirstAttempt_ReturnsSuccessWithOneAttempt()
    {
        var result = await RetryPolicy.ExecuteAsync(
            _ => Task.FromResult(true),
            logger: _loggerMock.Object,
            operationName: "TestOperation");

        Assert.True(result.Success);
        Assert.True(result.Value);
        Assert.Equal(1, result.Attempts);
    }

    [Fact]
    public async Task ExecuteAsync_SucceedsAfterRetries_ReturnsCorrectAttemptCount()
    {
        var attemptCount = 0;

        var result = await RetryPolicy.ExecuteAsync(
            _ =>
            {
                attemptCount++;
                return Task.FromResult(attemptCount >= 3); // Succeed on 3rd attempt
            },
            new RetryOptions { MaxRetries = 3, InitialDelay = TimeSpan.FromMilliseconds(10) },
            _loggerMock.Object,
            "TestOperation");

        Assert.True(result.Success);
        Assert.True(result.Value);
        Assert.Equal(3, result.Attempts);
    }

    [Fact]
    public async Task ExecuteAsync_ExceedsMaxRetries_ReturnsLastResult()
    {
        // When max retries are exhausted, the last result is returned as "Succeeded"
        // since the operation itself didn't throw - it just returned a retry-able value
        var result = await RetryPolicy.ExecuteAsync(
            _ => Task.FromResult(false), // Always returns false (should retry)
            new RetryOptions { MaxRetries = 2, InitialDelay = TimeSpan.FromMilliseconds(10) },
            _loggerMock.Object,
            "TestOperation");

        // The operation completed (didn't throw), so it's marked as "Succeeded"
        // even though the value indicates failure
        Assert.True(result.Success);
        Assert.False(result.Value); // The actual value is still false
        Assert.Equal(3, result.Attempts); // 1 initial + 2 retries
    }

    [Fact]
    public async Task ExecuteAsync_TransientException_RetriesAndSucceeds()
    {
        var attemptCount = 0;

        var result = await RetryPolicy.ExecuteAsync<bool>(
            _ =>
            {
                attemptCount++;
                if (attemptCount < 2)
                {
                    throw new TimeoutException("Transient timeout");
                }
                return Task.FromResult(true);
            },
            _ => false, // Don't retry on result
            new RetryOptions { MaxRetries = 3, InitialDelay = TimeSpan.FromMilliseconds(10) },
            _loggerMock.Object,
            "TestOperation");

        Assert.True(result.Success);
        Assert.Equal(2, result.Attempts);
    }

    [Fact]
    public async Task ExecuteAsync_NonTransientException_DoesNotRetry()
    {
        var attemptCount = 0;

        var result = await RetryPolicy.ExecuteAsync<bool>(
            _ =>
            {
                attemptCount++;
                throw new InvalidOperationException("Non-transient error");
            },
            _ => false,
            new RetryOptions { MaxRetries = 3, InitialDelay = TimeSpan.FromMilliseconds(10) },
            _loggerMock.Object,
            "TestOperation");

        Assert.False(result.Success);
        Assert.Equal(1, result.Attempts); // Should not retry
        Assert.IsType<InvalidOperationException>(result.LastException);
    }

    [Fact]
    public async Task ExecuteAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            RetryPolicy.ExecuteAsync(
                _ => Task.FromResult(true),
                cancellationToken: cts.Token));
    }

    [Theory]
    [InlineData(HttpStatusCode.RequestTimeout, true)]       // 408
    [InlineData(HttpStatusCode.TooManyRequests, true)]      // 429
    [InlineData(HttpStatusCode.InternalServerError, true)]  // 500
    [InlineData(HttpStatusCode.BadGateway, true)]           // 502
    [InlineData(HttpStatusCode.ServiceUnavailable, true)]   // 503
    [InlineData(HttpStatusCode.GatewayTimeout, true)]       // 504
    [InlineData(HttpStatusCode.OK, false)]                  // 200
    [InlineData(HttpStatusCode.BadRequest, false)]          // 400
    [InlineData(HttpStatusCode.Unauthorized, false)]        // 401
    [InlineData(HttpStatusCode.Forbidden, false)]           // 403
    [InlineData(HttpStatusCode.NotFound, false)]            // 404
    public void IsTransientHttpStatusCode_ReturnsExpectedValue(HttpStatusCode statusCode, bool expected)
    {
        var result = RetryPolicy.IsTransientHttpStatusCode(statusCode);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsTransientException_TimeoutException_ReturnsTrue()
    {
        var result = RetryPolicy.IsTransientException(new TimeoutException());
        Assert.True(result);
    }

    [Fact]
    public void IsTransientException_SocketException_ReturnsTrue()
    {
        var result = RetryPolicy.IsTransientException(new System.Net.Sockets.SocketException());
        Assert.True(result);
    }

    [Fact]
    public void IsTransientException_IOException_ReturnsTrue()
    {
        var result = RetryPolicy.IsTransientException(new IOException());
        Assert.True(result);
    }

    [Fact]
    public void IsTransientException_TaskCanceledException_ReturnsFalse()
    {
        var result = RetryPolicy.IsTransientException(new TaskCanceledException());
        Assert.False(result);
    }

    [Fact]
    public void IsTransientException_OperationCanceledException_ReturnsFalse()
    {
        var result = RetryPolicy.IsTransientException(new OperationCanceledException());
        Assert.False(result);
    }

    [Fact]
    public void IsTransientException_InvalidOperationException_ReturnsFalse()
    {
        var result = RetryPolicy.IsTransientException(new InvalidOperationException());
        Assert.False(result);
    }

    [Fact]
    public void IsTransientException_HttpRequestExceptionWith503_ReturnsTrue()
    {
        var ex = new HttpRequestException("Service unavailable", null, HttpStatusCode.ServiceUnavailable);
        var result = RetryPolicy.IsTransientException(ex);
        Assert.True(result);
    }

    [Fact]
    public void IsTransientException_HttpRequestExceptionWith400_ReturnsFalse()
    {
        var ex = new HttpRequestException("Bad request", null, HttpStatusCode.BadRequest);
        var result = RetryPolicy.IsTransientException(ex);
        Assert.False(result);
    }

    [Fact]
    public async Task ExecuteHttpAsync_TransientStatusCode_Retries()
    {
        var attemptCount = 0;

        var result = await RetryPolicy.ExecuteHttpAsync(
            _ =>
            {
                attemptCount++;
                if (attemptCount < 2)
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
                }
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            },
            new RetryOptions { MaxRetries = 3, InitialDelay = TimeSpan.FromMilliseconds(10) },
            _loggerMock.Object,
            "TestHttpOperation");

        Assert.True(result.Success);
        Assert.Equal(HttpStatusCode.OK, result.Value?.StatusCode);
        Assert.Equal(2, result.Attempts);
    }

    [Fact]
    public async Task ExecuteAsync_RecordsTotalDuration()
    {
        var result = await RetryPolicy.ExecuteAsync(
            async _ =>
            {
                await Task.Delay(100);
                return true;
            },
            logger: _loggerMock.Object,
            operationName: "TestOperation");

        // Allow some tolerance for timing variations
        Assert.True(result.TotalDuration.TotalMilliseconds >= 50,
            $"Expected duration >= 50ms but was {result.TotalDuration.TotalMilliseconds}ms");
    }

    [Fact]
    public async Task ExecuteAsync_WithCustomOptions_RespectsSettings()
    {
        var options = new RetryOptions
        {
            MaxRetries = 1,
            InitialDelay = TimeSpan.FromMilliseconds(10),
            MaxDelay = TimeSpan.FromMilliseconds(100),
            BackoffMultiplier = 2.0,
            AddJitter = false
        };

        var attemptCount = 0;
        var result = await RetryPolicy.ExecuteAsync(
            _ =>
            {
                attemptCount++;
                return Task.FromResult(false);
            },
            options,
            _loggerMock.Object,
            "TestOperation");

        // Operation completed without throwing, so it's "Succeeded" even with false value
        Assert.True(result.Success);
        Assert.False(result.Value);
        Assert.Equal(2, result.Attempts); // 1 initial + 1 retry (MaxRetries = 1)
    }
}
