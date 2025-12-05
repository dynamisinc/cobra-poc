using System.Collections.Concurrent;

namespace CobraAPI.TeamsBot.Services;

/// <summary>
/// Simple metrics collection for bot monitoring.
/// Exposes counters and gauges for Prometheus-style scraping.
/// </summary>
public interface IBotMetrics
{
    void IncrementMessagesReceived(string channelId = "teams");
    void IncrementMessagesSent(string channelId = "teams");
    void IncrementMessagesFailed(string channelId = "teams", string reason = "unknown");
    void IncrementWebhooksSent(bool success);
    void RecordMessageLatency(double milliseconds);
    void SetActiveConversations(int count);
    BotMetricsSnapshot GetSnapshot();
}

/// <summary>
/// Snapshot of current metrics for reporting.
/// </summary>
public class BotMetricsSnapshot
{
    public long MessagesReceived { get; init; }
    public long MessagesSent { get; init; }
    public long MessagesFailed { get; init; }
    public long WebhooksSucceeded { get; init; }
    public long WebhooksFailed { get; init; }
    public int ActiveConversations { get; init; }
    public double AverageLatencyMs { get; init; }
    public double MaxLatencyMs { get; init; }
    public DateTime StartTime { get; init; }
    public TimeSpan Uptime { get; init; }
    public Dictionary<string, long> MessagesReceivedByChannel { get; init; } = new();
    public Dictionary<string, long> FailuresByReason { get; init; } = new();
}

/// <summary>
/// Thread-safe implementation of bot metrics collection.
/// </summary>
public class BotMetrics : IBotMetrics
{
    private long _messagesReceived;
    private long _messagesSent;
    private long _messagesFailed;
    private long _webhooksSucceeded;
    private long _webhooksFailed;
    private int _activeConversations;
    private readonly DateTime _startTime = DateTime.UtcNow;

    private readonly ConcurrentDictionary<string, long> _messagesReceivedByChannel = new();
    private readonly ConcurrentDictionary<string, long> _failuresByReason = new();

    // Rolling latency tracking (last 100 samples)
    private readonly ConcurrentQueue<double> _latencySamples = new();
    private const int MaxLatencySamples = 100;

    public void IncrementMessagesReceived(string channelId = "teams")
    {
        Interlocked.Increment(ref _messagesReceived);
        _messagesReceivedByChannel.AddOrUpdate(channelId, 1, (_, count) => count + 1);
    }

    public void IncrementMessagesSent(string channelId = "teams")
    {
        Interlocked.Increment(ref _messagesSent);
    }

    public void IncrementMessagesFailed(string channelId = "teams", string reason = "unknown")
    {
        Interlocked.Increment(ref _messagesFailed);
        _failuresByReason.AddOrUpdate(reason, 1, (_, count) => count + 1);
    }

    public void IncrementWebhooksSent(bool success)
    {
        if (success)
            Interlocked.Increment(ref _webhooksSucceeded);
        else
            Interlocked.Increment(ref _webhooksFailed);
    }

    public void RecordMessageLatency(double milliseconds)
    {
        _latencySamples.Enqueue(milliseconds);

        // Keep only the last N samples
        while (_latencySamples.Count > MaxLatencySamples)
        {
            _latencySamples.TryDequeue(out _);
        }
    }

    public void SetActiveConversations(int count)
    {
        Interlocked.Exchange(ref _activeConversations, count);
    }

    public BotMetricsSnapshot GetSnapshot()
    {
        var samples = _latencySamples.ToArray();
        var avgLatency = samples.Length > 0 ? samples.Average() : 0;
        var maxLatency = samples.Length > 0 ? samples.Max() : 0;

        return new BotMetricsSnapshot
        {
            MessagesReceived = Interlocked.Read(ref _messagesReceived),
            MessagesSent = Interlocked.Read(ref _messagesSent),
            MessagesFailed = Interlocked.Read(ref _messagesFailed),
            WebhooksSucceeded = Interlocked.Read(ref _webhooksSucceeded),
            WebhooksFailed = Interlocked.Read(ref _webhooksFailed),
            ActiveConversations = _activeConversations,
            AverageLatencyMs = Math.Round(avgLatency, 2),
            MaxLatencyMs = Math.Round(maxLatency, 2),
            StartTime = _startTime,
            Uptime = DateTime.UtcNow - _startTime,
            MessagesReceivedByChannel = new Dictionary<string, long>(_messagesReceivedByChannel),
            FailuresByReason = new Dictionary<string, long>(_failuresByReason)
        };
    }
}
