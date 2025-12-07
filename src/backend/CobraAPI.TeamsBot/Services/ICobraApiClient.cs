using CobraAPI.TeamsBot.Models;

namespace CobraAPI.TeamsBot.Services;

/// <summary>
/// Client interface for communicating with CobraAPI.
/// TeamsBot uses this to forward inbound Teams messages to COBRA
/// and to store/retrieve ConversationReferences for stateless operation.
/// </summary>
public interface ICobraApiClient
{
    /// <summary>
    /// Checks if CobraAPI is reachable and healthy.
    /// </summary>
    /// <returns>True if the health check passes.</returns>
    Task<bool> CheckHealthAsync();

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

    // === Stateless Architecture Methods (UC-TI-029) ===

    /// <summary>
    /// Stores or updates a ConversationReference in CobraAPI.
    /// Called on every incoming message to keep the reference current.
    /// </summary>
    /// <param name="request">The conversation reference data.</param>
    /// <returns>The response with mapping ID, or null on failure.</returns>
    Task<StoreConversationReferenceResult?> StoreConversationReferenceAsync(StoreConversationReferenceRequest request);

    /// <summary>
    /// Gets a ConversationReference from CobraAPI by conversation ID.
    /// </summary>
    /// <param name="conversationId">The Teams conversation ID.</param>
    /// <returns>The conversation reference JSON, or null if not found.</returns>
    Task<GetConversationReferenceResult?> GetConversationReferenceAsync(string conversationId);

    /// <summary>
    /// Notifies CobraAPI that the bot has been removed from a Teams conversation.
    /// This deactivates the corresponding ExternalChannelMapping.
    /// </summary>
    /// <param name="conversationId">The Teams conversation ID.</param>
    /// <param name="removedBy">Optional name of the user who removed the bot.</param>
    /// <returns>True if the notification was processed successfully.</returns>
    Task<bool> NotifyBotRemovedAsync(string conversationId, string? removedBy = null);

    // === Bot Command Support Methods ===

    /// <summary>
    /// Gets a list of active events that can be linked to this Teams channel.
    /// </summary>
    /// <param name="tenantId">Optional Teams tenant ID to filter events by organization.</param>
    /// <returns>List of events available for linking.</returns>
    Task<List<EventSummary>?> GetAvailableEventsAsync(string? tenantId = null);

    /// <summary>
    /// Links a Teams connector to a COBRA event.
    /// </summary>
    /// <param name="mappingId">The connector mapping ID.</param>
    /// <param name="eventId">The event ID to link to.</param>
    /// <returns>Link result with event details, or null on failure.</returns>
    Task<LinkResult?> LinkConnectorToEventAsync(Guid mappingId, Guid eventId);

    /// <summary>
    /// Unlinks a Teams connector from its current event.
    /// </summary>
    /// <param name="mappingId">The connector mapping ID.</param>
    /// <returns>True if unlinking was successful.</returns>
    Task<bool> UnlinkConnectorAsync(Guid mappingId);

    /// <summary>
    /// Gets the current connector status including linked event info.
    /// </summary>
    /// <param name="conversationId">The Teams conversation ID.</param>
    /// <returns>Connector status information.</returns>
    Task<ConnectorStatus?> GetConnectorStatusAsync(string conversationId);

    /// <summary>
    /// Creates a new COBRA channel from the bot.
    /// The channel will be created in the event linked to this connector.
    /// </summary>
    /// <param name="mappingId">The connector mapping ID.</param>
    /// <param name="channelName">Name for the new channel.</param>
    /// <param name="description">Optional description.</param>
    /// <param name="createdByName">Name of the user requesting creation.</param>
    /// <returns>Channel creation result, or null on failure.</returns>
    Task<CreateChannelResult?> CreateChannelAsync(Guid mappingId, string channelName, string? description = null, string? createdByName = null);
}

/// <summary>
/// Summary of an event available for linking.
/// </summary>
public class EventSummary
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Result from linking a connector to an event.
/// </summary>
public class LinkResult
{
    public bool Success { get; set; }
    public Guid MappingId { get; set; }
    public Guid EventId { get; set; }
    public string? EventName { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Status information for a Teams connector.
/// </summary>
public class ConnectorStatus
{
    public Guid MappingId { get; set; }
    public string? DisplayName { get; set; }
    public bool IsActive { get; set; }
    public bool IsLinked { get; set; }
    public Guid? LinkedEventId { get; set; }
    public string? LinkedEventName { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Result from creating a channel via bot command.
/// </summary>
public class CreateChannelResult
{
    public bool Success { get; set; }
    public Guid ChannelId { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public Guid EventId { get; set; }
    public string? EventName { get; set; }
    public string? Message { get; set; }

    /// <summary>
    /// The external channel mapping ID the channel is linked to.
    /// This enables bidirectional messaging between the COBRA channel and this Teams connector.
    /// </summary>
    public Guid? LinkedMappingId { get; set; }

    /// <summary>
    /// True if an existing channel was linked (rather than a new channel being created).
    /// </summary>
    public bool WasExisting { get; set; }
}

/// <summary>
/// Request to store a conversation reference in CobraAPI.
/// </summary>
public class StoreConversationReferenceRequest
{
    public required string ConversationId { get; init; }
    public required string ConversationReferenceJson { get; init; }
    public string? TenantId { get; init; }
    public string? ChannelName { get; init; }
    public string? InstalledByName { get; init; }
    public bool IsEmulator { get; init; }
}

/// <summary>
/// Result from storing a conversation reference.
/// </summary>
public class StoreConversationReferenceResult
{
    public Guid MappingId { get; set; }
    public bool IsNewMapping { get; set; }
}

/// <summary>
/// Result from getting a conversation reference.
/// </summary>
public class GetConversationReferenceResult
{
    public Guid MappingId { get; set; }
    public string? ConversationReferenceJson { get; set; }
    public bool IsActive { get; set; }
}
