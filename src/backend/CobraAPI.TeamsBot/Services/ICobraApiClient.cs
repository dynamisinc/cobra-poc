using CobraAPI.TeamsBot.Models;

namespace CobraAPI.TeamsBot.Services;

/// <summary>
/// Client interface for communicating with CobraAPI.
/// TeamsBot uses this to forward inbound Teams messages to COBRA.
/// </summary>
public interface ICobraApiClient
{
    /// <summary>
    /// Sends a Teams message to the CobraAPI webhook endpoint.
    /// </summary>
    /// <param name="mappingId">The external channel mapping ID in COBRA.</param>
    /// <param name="payload">The Teams message payload.</param>
    /// <returns>True if the webhook was processed successfully.</returns>
    Task<bool> SendWebhookAsync(Guid mappingId, TeamsWebhookPayload payload);

    /// <summary>
    /// Sends a Teams message to CobraAPI when there's no mapping established yet.
    /// Used for initial connection/linking flow.
    /// </summary>
    /// <param name="conversationId">The Teams conversation ID.</param>
    /// <param name="payload">The Teams message payload.</param>
    /// <returns>True if processed successfully.</returns>
    Task<bool> SendUnmappedMessageAsync(string conversationId, TeamsWebhookPayload payload);

    /// <summary>
    /// Gets the channel mapping ID for a Teams conversation.
    /// </summary>
    /// <param name="conversationId">The Teams conversation ID.</param>
    /// <returns>The mapping ID if found, null otherwise.</returns>
    Task<Guid?> GetMappingIdForConversationAsync(string conversationId);
}
