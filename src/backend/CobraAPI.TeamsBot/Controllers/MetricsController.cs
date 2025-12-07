using System.Text;
using CobraAPI.TeamsBot.Services;
using Microsoft.AspNetCore.Mvc;

namespace CobraAPI.TeamsBot.Controllers;

/// <summary>
/// Metrics endpoint for monitoring and alerting systems.
/// Supports both JSON and Prometheus text formats.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class MetricsController : ControllerBase
{
    private readonly IBotMetrics _metrics;
    private readonly IConversationReferenceService _conversationReferenceService;

    public MetricsController(
        IBotMetrics metrics,
        IConversationReferenceService conversationReferenceService)
    {
        _metrics = metrics;
        _conversationReferenceService = conversationReferenceService;
    }

    /// <summary>
    /// Get metrics in JSON format.
    /// </summary>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetrics()
    {
        // Update active conversations count
        var references = await _conversationReferenceService.GetAllAsync();
        _metrics.SetActiveConversations(references.Count);

        var snapshot = _metrics.GetSnapshot();
        return Ok(snapshot);
    }

    /// <summary>
    /// Get metrics in Prometheus text exposition format.
    /// Use this endpoint for Prometheus scraping.
    /// </summary>
    [HttpGet("prometheus")]
    [Produces("text/plain")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPrometheusMetrics()
    {
        // Update active conversations count
        var references = await _conversationReferenceService.GetAllAsync();
        _metrics.SetActiveConversations(references.Count);

        var snapshot = _metrics.GetSnapshot();
        var sb = new StringBuilder();

        // Standard Prometheus format
        sb.AppendLine("# HELP teamsbot_messages_received_total Total messages received from Teams");
        sb.AppendLine("# TYPE teamsbot_messages_received_total counter");
        sb.AppendLine($"teamsbot_messages_received_total {snapshot.MessagesReceived}");

        sb.AppendLine("# HELP teamsbot_messages_sent_total Total messages sent to Teams");
        sb.AppendLine("# TYPE teamsbot_messages_sent_total counter");
        sb.AppendLine($"teamsbot_messages_sent_total {snapshot.MessagesSent}");

        sb.AppendLine("# HELP teamsbot_messages_failed_total Total messages that failed to send");
        sb.AppendLine("# TYPE teamsbot_messages_failed_total counter");
        sb.AppendLine($"teamsbot_messages_failed_total {snapshot.MessagesFailed}");

        sb.AppendLine("# HELP teamsbot_webhooks_total Total webhooks sent to CobraAPI");
        sb.AppendLine("# TYPE teamsbot_webhooks_total counter");
        sb.AppendLine($"teamsbot_webhooks_total{{status=\"success\"}} {snapshot.WebhooksSucceeded}");
        sb.AppendLine($"teamsbot_webhooks_total{{status=\"failed\"}} {snapshot.WebhooksFailed}");

        sb.AppendLine("# HELP teamsbot_active_conversations Current number of active conversation references");
        sb.AppendLine("# TYPE teamsbot_active_conversations gauge");
        sb.AppendLine($"teamsbot_active_conversations {snapshot.ActiveConversations}");

        sb.AppendLine("# HELP teamsbot_message_latency_avg_ms Average message processing latency");
        sb.AppendLine("# TYPE teamsbot_message_latency_avg_ms gauge");
        sb.AppendLine($"teamsbot_message_latency_avg_ms {snapshot.AverageLatencyMs}");

        sb.AppendLine("# HELP teamsbot_message_latency_max_ms Maximum message processing latency");
        sb.AppendLine("# TYPE teamsbot_message_latency_max_ms gauge");
        sb.AppendLine($"teamsbot_message_latency_max_ms {snapshot.MaxLatencyMs}");

        sb.AppendLine("# HELP teamsbot_uptime_seconds Bot uptime in seconds");
        sb.AppendLine("# TYPE teamsbot_uptime_seconds gauge");
        sb.AppendLine($"teamsbot_uptime_seconds {snapshot.Uptime.TotalSeconds:F0}");

        // Per-channel breakdown
        sb.AppendLine("# HELP teamsbot_messages_by_channel_total Messages received by channel type");
        sb.AppendLine("# TYPE teamsbot_messages_by_channel_total counter");
        foreach (var (channel, count) in snapshot.MessagesReceivedByChannel)
        {
            sb.AppendLine($"teamsbot_messages_by_channel_total{{channel=\"{channel}\"}} {count}");
        }

        // Failure reasons
        if (snapshot.FailuresByReason.Count > 0)
        {
            sb.AppendLine("# HELP teamsbot_failures_by_reason_total Failures by reason");
            sb.AppendLine("# TYPE teamsbot_failures_by_reason_total counter");
            foreach (var (reason, count) in snapshot.FailuresByReason)
            {
                sb.AppendLine($"teamsbot_failures_by_reason_total{{reason=\"{reason}\"}} {count}");
            }
        }

        return Content(sb.ToString(), "text/plain; version=0.0.4; charset=utf-8");
    }
}
