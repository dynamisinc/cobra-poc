using CobraAPI.TeamsBot.Services;

namespace CobraAPI.TeamsBot.Tests.Services;

public class BotMetricsTests
{
    private readonly BotMetrics _metrics;

    public BotMetricsTests()
    {
        _metrics = new BotMetrics();
    }

    [Fact]
    public void IncrementMessagesReceived_IncrementsCounter()
    {
        _metrics.IncrementMessagesReceived();
        _metrics.IncrementMessagesReceived();
        _metrics.IncrementMessagesReceived();

        var snapshot = _metrics.GetSnapshot();

        Assert.Equal(3, snapshot.MessagesReceived);
    }

    [Fact]
    public void IncrementMessagesReceived_TracksChannelBreakdown()
    {
        _metrics.IncrementMessagesReceived("teams");
        _metrics.IncrementMessagesReceived("teams");
        _metrics.IncrementMessagesReceived("emulator");

        var snapshot = _metrics.GetSnapshot();

        Assert.Equal(3, snapshot.MessagesReceived);
        Assert.Equal(2, snapshot.MessagesReceivedByChannel["teams"]);
        Assert.Equal(1, snapshot.MessagesReceivedByChannel["emulator"]);
    }

    [Fact]
    public void IncrementMessagesSent_IncrementsCounter()
    {
        _metrics.IncrementMessagesSent();
        _metrics.IncrementMessagesSent();

        var snapshot = _metrics.GetSnapshot();

        Assert.Equal(2, snapshot.MessagesSent);
    }

    [Fact]
    public void IncrementMessagesFailed_IncrementsCounterAndTracksReason()
    {
        _metrics.IncrementMessagesFailed("teams", "timeout");
        _metrics.IncrementMessagesFailed("teams", "timeout");
        _metrics.IncrementMessagesFailed("teams", "auth_error");

        var snapshot = _metrics.GetSnapshot();

        Assert.Equal(3, snapshot.MessagesFailed);
        Assert.Equal(2, snapshot.FailuresByReason["timeout"]);
        Assert.Equal(1, snapshot.FailuresByReason["auth_error"]);
    }

    [Fact]
    public void IncrementWebhooksSent_Success_IncrementsSuccessCounter()
    {
        _metrics.IncrementWebhooksSent(true);
        _metrics.IncrementWebhooksSent(true);

        var snapshot = _metrics.GetSnapshot();

        Assert.Equal(2, snapshot.WebhooksSucceeded);
        Assert.Equal(0, snapshot.WebhooksFailed);
    }

    [Fact]
    public void IncrementWebhooksSent_Failure_IncrementsFailureCounter()
    {
        _metrics.IncrementWebhooksSent(false);
        _metrics.IncrementWebhooksSent(false);
        _metrics.IncrementWebhooksSent(true);

        var snapshot = _metrics.GetSnapshot();

        Assert.Equal(1, snapshot.WebhooksSucceeded);
        Assert.Equal(2, snapshot.WebhooksFailed);
    }

    [Fact]
    public void RecordMessageLatency_CalculatesAverageAndMax()
    {
        _metrics.RecordMessageLatency(100);
        _metrics.RecordMessageLatency(200);
        _metrics.RecordMessageLatency(300);

        var snapshot = _metrics.GetSnapshot();

        Assert.Equal(200, snapshot.AverageLatencyMs); // (100 + 200 + 300) / 3
        Assert.Equal(300, snapshot.MaxLatencyMs);
    }

    [Fact]
    public void RecordMessageLatency_NoSamples_ReturnsZero()
    {
        var snapshot = _metrics.GetSnapshot();

        Assert.Equal(0, snapshot.AverageLatencyMs);
        Assert.Equal(0, snapshot.MaxLatencyMs);
    }

    [Fact]
    public void SetActiveConversations_SetsGauge()
    {
        _metrics.SetActiveConversations(10);
        var snapshot1 = _metrics.GetSnapshot();
        Assert.Equal(10, snapshot1.ActiveConversations);

        _metrics.SetActiveConversations(5);
        var snapshot2 = _metrics.GetSnapshot();
        Assert.Equal(5, snapshot2.ActiveConversations);
    }

    [Fact]
    public void GetSnapshot_IncludesUptime()
    {
        var snapshot = _metrics.GetSnapshot();

        Assert.True(snapshot.Uptime.TotalMilliseconds > 0);
        Assert.True(snapshot.StartTime <= DateTime.UtcNow);
    }

    [Fact]
    public void GetSnapshot_ReturnsIndependentCopies()
    {
        _metrics.IncrementMessagesReceived("teams");
        var snapshot1 = _metrics.GetSnapshot();

        _metrics.IncrementMessagesReceived("teams");
        var snapshot2 = _metrics.GetSnapshot();

        // Snapshots should be independent
        Assert.Equal(1, snapshot1.MessagesReceived);
        Assert.Equal(2, snapshot2.MessagesReceived);
    }

    [Fact]
    public async Task Metrics_ThreadSafe_ConcurrentUpdates()
    {
        var tasks = new List<Task>();

        // Simulate concurrent metric updates
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                _metrics.IncrementMessagesReceived();
                _metrics.IncrementMessagesSent();
                _metrics.IncrementWebhooksSent(true);
                _metrics.RecordMessageLatency(50);
            }));
        }

        await Task.WhenAll(tasks);

        var snapshot = _metrics.GetSnapshot();

        Assert.Equal(100, snapshot.MessagesReceived);
        Assert.Equal(100, snapshot.MessagesSent);
        Assert.Equal(100, snapshot.WebhooksSucceeded);
    }

    [Fact]
    public void RecordMessageLatency_LimitsToMaxSamples()
    {
        // Record more than MaxLatencySamples (100)
        for (int i = 1; i <= 150; i++)
        {
            _metrics.RecordMessageLatency(i);
        }

        var snapshot = _metrics.GetSnapshot();

        // Average should be based on last 100 samples (51-150)
        // Average of 51..150 = (51+150)/2 = 100.5
        Assert.True(snapshot.AverageLatencyMs >= 100 && snapshot.AverageLatencyMs <= 101);
        Assert.Equal(150, snapshot.MaxLatencyMs);
    }

    [Fact]
    public void IncrementMessagesFailed_WithDefaultReason_TracksUnknown()
    {
        _metrics.IncrementMessagesFailed();

        var snapshot = _metrics.GetSnapshot();

        Assert.Equal(1, snapshot.MessagesFailed);
        Assert.Equal(1, snapshot.FailuresByReason["unknown"]);
    }

    [Fact]
    public void IncrementMessagesReceived_WithDefaultChannel_TracksTeams()
    {
        _metrics.IncrementMessagesReceived();

        var snapshot = _metrics.GetSnapshot();

        Assert.Equal(1, snapshot.MessagesReceivedByChannel["teams"]);
    }
}
