using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CobraAPI.Core.Data;
using CobraAPI.Core.Models;
using CobraAPI.Tools.Chat.Models.DTOs;
using CobraAPI.Tools.Chat.Models.Entities;
using CobraAPI.Tools.Chat.Services;

namespace CobraAPI.Tools.Chat.Controllers;

/// <summary>
/// API endpoints for Teams bot stateless architecture.
/// Manages ConversationReferences and connector metadata.
/// </summary>
[ApiController]
[Route("api/chat/[controller]")]
[AllowAnonymous] // POC: No auth required. Production: Add API key or JWT validation.
public class TeamsController : ControllerBase
{
    private readonly CobraDbContext _dbContext;
    private readonly IExternalMessagingService _externalMessagingService;
    private readonly IChannelService _channelService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TeamsController> _logger;

    public TeamsController(
        CobraDbContext dbContext,
        IExternalMessagingService externalMessagingService,
        IChannelService channelService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<TeamsController> logger)
    {
        _dbContext = dbContext;
        _externalMessagingService = externalMessagingService;
        _channelService = channelService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    private UserContext? GetUserContext()
    {
        return _httpContextAccessor.HttpContext?.Items["UserContext"] as UserContext;
    }

    /// <summary>
    /// Store or update a ConversationReference for a Teams conversation.
    /// Called by TeamsBot on every incoming message to keep the reference current.
    /// </summary>
    [HttpPut("conversation-reference")]
    [ProducesResponseType(typeof(StoreConversationReferenceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> StoreConversationReference(
        [FromBody] StoreConversationReferenceRequest request)
    {
        _logger.LogDebug(
            "Storing conversation reference for {ConversationId}, TenantId: {TenantId}, IsEmulator: {IsEmulator}",
            request.ConversationId, request.TenantId, request.IsEmulator);

        // Find existing mapping by conversation ID
        var mapping = await _dbContext.ExternalChannelMappings
            .FirstOrDefaultAsync(m =>
                m.Platform == ExternalPlatform.Teams &&
                m.ExternalGroupId == request.ConversationId);

        var isNewMapping = mapping == null;

        if (mapping == null)
        {
            // Create new mapping - EventId is null (unlinked) until user explicitly links to an event
            // This allows the connector to be registered and available for linking via the admin UI
            mapping = new ExternalChannelMapping
            {
                Id = Guid.NewGuid(),
                EventId = null, // Unlinked - user must explicitly link to an event
                Platform = ExternalPlatform.Teams,
                ExternalGroupId = request.ConversationId,
                ExternalGroupName = request.ChannelName ?? $"Teams {(request.IsEmulator ? "Emulator" : "Channel")}",
                BotId = string.Empty, // Not used for Teams - ConversationReference has bot info
                WebhookSecret = Guid.NewGuid().ToString("N"), // Generate for consistency
                CreatedBy = "TeamsBot",
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.ExternalChannelMappings.Add(mapping);

            _logger.LogInformation(
                "Created new unlinked Teams connector {MappingId} for conversation {ConversationId}. " +
                "Connector must be linked to an event via admin UI.",
                mapping.Id, request.ConversationId);
        }

        // Update ConversationReference and metadata
        mapping.ConversationReferenceJson = request.ConversationReferenceJson;
        mapping.TenantId = request.TenantId;
        mapping.LastActivityAt = DateTime.UtcNow;
        mapping.IsEmulator = request.IsEmulator;
        mapping.LastModifiedBy = "TeamsBot";
        mapping.LastModifiedAt = DateTime.UtcNow;

        // Set InstalledByName only if not already set
        if (string.IsNullOrEmpty(mapping.InstalledByName) && !string.IsNullOrEmpty(request.InstalledByName))
        {
            mapping.InstalledByName = request.InstalledByName;
        }

        await _dbContext.SaveChangesAsync();

        return Ok(new StoreConversationReferenceResponse
        {
            MappingId = mapping.Id,
            IsNewMapping = isNewMapping
        });
    }

    /// <summary>
    /// Get a ConversationReference by conversation ID.
    /// Used by TeamsBot when receiving messages to look up the mapping.
    /// </summary>
    [HttpGet("conversation-reference/{conversationId}")]
    [ProducesResponseType(typeof(GetConversationReferenceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConversationReference(string conversationId)
    {
        var decodedId = Uri.UnescapeDataString(conversationId);

        var mapping = await _dbContext.ExternalChannelMappings
            .FirstOrDefaultAsync(m =>
                m.Platform == ExternalPlatform.Teams &&
                m.ExternalGroupId == decodedId);

        if (mapping == null)
        {
            return NotFound(new { error = "No mapping found for this conversation" });
        }

        return Ok(new GetConversationReferenceResponse
        {
            MappingId = mapping.Id,
            ConversationReferenceJson = mapping.ConversationReferenceJson,
            IsActive = mapping.IsActive
        });
    }

    /// <summary>
    /// List all Teams connectors with metadata.
    /// Used by admin UI to manage connectors.
    /// </summary>
    [HttpGet("mappings")]
    [ProducesResponseType(typeof(ListTeamsConnectorsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListMappings(
        [FromQuery] bool? isEmulator = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int? staleDays = null)
    {
        var query = _dbContext.ExternalChannelMappings
            .Include(m => m.Event)
            .Where(m => m.Platform == ExternalPlatform.Teams);

        if (isEmulator.HasValue)
        {
            query = query.Where(m => m.IsEmulator == isEmulator.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(m => m.IsActive == isActive.Value);
        }

        if (staleDays.HasValue)
        {
            var cutoff = DateTime.UtcNow.AddDays(-staleDays.Value);
            query = query.Where(m => m.LastActivityAt == null || m.LastActivityAt < cutoff);
        }

        var rawMappings = await query
            .OrderByDescending(m => m.LastActivityAt)
            .ThenByDescending(m => m.CreatedAt)
            .Select(m => new
            {
                m.Id,
                m.ExternalGroupName,
                m.ExternalGroupId,
                m.TenantId,
                m.LastActivityAt,
                m.InstalledByName,
                m.IsEmulator,
                m.IsActive,
                HasConversationReference = m.ConversationReferenceJson != null,
                m.CreatedAt,
                m.EventId,
                EventName = m.Event != null ? m.Event.Name : null
            })
            .ToListAsync();

        // Ensure dates are properly marked as UTC for JSON serialization
        // (EF Core returns DateTime with Kind=Unspecified, causing browser to misinterpret as local time)
        var mappings = rawMappings.Select(m => new TeamsConnectorDto
        {
            MappingId = m.Id,
            DisplayName = m.ExternalGroupName,
            ConversationId = m.ExternalGroupId,
            TenantId = m.TenantId,
            LastActivityAt = m.LastActivityAt.HasValue
                ? DateTime.SpecifyKind(m.LastActivityAt.Value, DateTimeKind.Utc)
                : null,
            InstalledByName = m.InstalledByName,
            IsEmulator = m.IsEmulator,
            IsActive = m.IsActive,
            HasConversationReference = m.HasConversationReference,
            CreatedAt = DateTime.SpecifyKind(m.CreatedAt, DateTimeKind.Utc),
            LinkedEventId = m.EventId,
            LinkedEventName = m.EventName,
            IsLinked = m.EventId != null
        }).ToList();

        return Ok(new ListTeamsConnectorsResponse
        {
            Count = mappings.Count,
            Connectors = mappings
        });
    }

    /// <summary>
    /// Rename a Teams connector (update display name).
    /// </summary>
    [HttpPatch("mappings/{mappingId:guid}/name")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RenameConnector(Guid mappingId, [FromBody] RenameConnectorRequest request)
    {
        var mapping = await _dbContext.ExternalChannelMappings
            .FirstOrDefaultAsync(m => m.Id == mappingId && m.Platform == ExternalPlatform.Teams);

        if (mapping == null)
        {
            return NotFound(new { error = "Mapping not found" });
        }

        mapping.ExternalGroupName = request.DisplayName;
        mapping.LastModifiedBy = "Admin";
        mapping.LastModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Renamed Teams connector {MappingId} to '{DisplayName}'",
            mappingId, request.DisplayName);

        return Ok(new { message = "Connector renamed", mappingId, displayName = request.DisplayName });
    }

    /// <summary>
    /// Delete (deactivate) a Teams connector.
    /// Uses soft delete - sets IsActive = false.
    /// </summary>
    [HttpDelete("mappings/{mappingId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteConnector(Guid mappingId)
    {
        var mapping = await _dbContext.ExternalChannelMappings
            .FirstOrDefaultAsync(m => m.Id == mappingId && m.Platform == ExternalPlatform.Teams);

        if (mapping == null)
        {
            return NotFound(new { error = "Mapping not found" });
        }

        mapping.IsActive = false;
        mapping.LastModifiedBy = "Admin";
        mapping.LastModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Deactivated Teams connector {MappingId}", mappingId);

        return Ok(new { message = "Connector deactivated", mappingId });
    }

    /// <summary>
    /// Bulk delete stale connectors.
    /// Removes connectors that haven't had activity in the specified number of days.
    /// </summary>
    [HttpDelete("mappings/stale")]
    [ProducesResponseType(typeof(CleanupResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteStaleConnectors([FromQuery] int inactiveDays = 30)
    {
        var cutoff = DateTime.UtcNow.AddDays(-inactiveDays);

        var staleConnectors = await _dbContext.ExternalChannelMappings
            .Where(m =>
                m.Platform == ExternalPlatform.Teams &&
                m.IsActive &&
                (m.LastActivityAt == null || m.LastActivityAt < cutoff))
            .ToListAsync();

        var deletedIds = new List<Guid>();

        foreach (var connector in staleConnectors)
        {
            connector.IsActive = false;
            connector.LastModifiedBy = "StaleCleanup";
            connector.LastModifiedAt = DateTime.UtcNow;
            deletedIds.Add(connector.Id);
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Cleaned up {Count} stale Teams connectors (inactive for {Days}+ days)",
            deletedIds.Count, inactiveDays);

        return Ok(new CleanupResponse
        {
            DeletedCount = deletedIds.Count,
            DeletedMappingIds = deletedIds
        });
    }

    /// <summary>
    /// Link an unlinked Teams connector to an event.
    /// This assigns the connector to a specific event so messages can flow between them.
    /// </summary>
    [HttpPost("mappings/{mappingId:guid}/link")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LinkConnectorToEvent(Guid mappingId, [FromBody] LinkConnectorRequest request)
    {
        var mapping = await _dbContext.ExternalChannelMappings
            .FirstOrDefaultAsync(m => m.Id == mappingId && m.Platform == ExternalPlatform.Teams);

        if (mapping == null)
        {
            return NotFound(new { error = "Connector not found" });
        }

        // Validate the event exists
        var eventEntity = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == request.EventId);
        if (eventEntity == null)
        {
            return NotFound(new { error = "Event not found" });
        }

        var userContext = GetUserContext();
        var previousEventId = mapping.EventId;

        mapping.EventId = request.EventId;
        mapping.LastModifiedBy = userContext?.Email ?? "Admin";
        mapping.LastModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Linked Teams connector {MappingId} to event {EventId} ({EventName}). Previous: {PreviousEventId}",
            mappingId, request.EventId, eventEntity.Name, previousEventId?.ToString() ?? "none");

        return Ok(new
        {
            message = "Connector linked to event",
            mappingId,
            eventId = request.EventId,
            eventName = eventEntity.Name
        });
    }

    /// <summary>
    /// Unlink a Teams connector from its event (sets EventId to null).
    /// The connector will remain registered but not associated with any event.
    /// </summary>
    [HttpPost("mappings/{mappingId:guid}/unlink")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlinkConnector(Guid mappingId)
    {
        var mapping = await _dbContext.ExternalChannelMappings
            .Include(m => m.Event)
            .FirstOrDefaultAsync(m => m.Id == mappingId && m.Platform == ExternalPlatform.Teams);

        if (mapping == null)
        {
            return NotFound(new { error = "Connector not found" });
        }

        var userContext = GetUserContext();
        var previousEventName = mapping.Event?.Name ?? "none";

        mapping.EventId = null;
        mapping.LastModifiedBy = userContext?.Email ?? "Admin";
        mapping.LastModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Unlinked Teams connector {MappingId} from event {EventName}",
            mappingId, previousEventName);

        return Ok(new { message = "Connector unlinked from event", mappingId });
    }

    /// <summary>
    /// Reactivate a previously deactivated connector.
    /// </summary>
    [HttpPost("mappings/{mappingId:guid}/reactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReactivateConnector(Guid mappingId)
    {
        var mapping = await _dbContext.ExternalChannelMappings
            .FirstOrDefaultAsync(m => m.Id == mappingId && m.Platform == ExternalPlatform.Teams);

        if (mapping == null)
        {
            return NotFound(new { error = "Mapping not found" });
        }

        mapping.IsActive = true;
        mapping.LastModifiedBy = "Admin";
        mapping.LastModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Reactivated Teams connector {MappingId}", mappingId);

        return Ok(new { message = "Connector reactivated", mappingId });
    }

    /// <summary>
    /// Broadcast an announcement to all Teams channels for an event.
    /// This sends the announcement to ALL connected Teams channels, not just one.
    /// </summary>
    [HttpPost("announcements/broadcast")]
    [ProducesResponseType(typeof(AnnouncementBroadcastResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BroadcastAnnouncement([FromBody] AnnouncementBroadcastRequest request)
    {
        // Validate event exists
        var eventExists = await _dbContext.Events.AnyAsync(e => e.Id == request.EventId);
        if (!eventExists)
        {
            return NotFound(new { error = "Event not found" });
        }

        // Get sender name from user context or request
        var userContext = GetUserContext();
        var senderName = request.SenderName ?? userContext?.FullName ?? "COBRA";

        _logger.LogInformation(
            "Broadcasting announcement '{Title}' to Teams for event {EventId} by {Sender}",
            request.Title, request.EventId, senderName);

        var sentCount = await _externalMessagingService.BroadcastAnnouncementToTeamsAsync(
            request.EventId,
            request.Title,
            request.Message,
            senderName,
            request.Priority ?? "normal");

        return Ok(new AnnouncementBroadcastResponse
        {
            Success = sentCount > 0,
            ChannelsSent = sentCount,
            Message = sentCount > 0
                ? $"Announcement sent to {sentCount} Teams channel(s)"
                : "No active Teams channels found for this event"
        });
    }

    /// <summary>
    /// Handle notification that bot has been removed from a Teams conversation.
    /// This deactivates the ExternalChannelMapping and clears the ConversationReference.
    /// Called by TeamsBot when OnMembersRemovedAsync detects bot removal.
    /// UC-TI-021: Bot Removal Detection.
    /// </summary>
    [HttpPost("bot-removed/{conversationId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> NotifyBotRemoved(
        string conversationId,
        [FromBody] BotRemovedRequest? request = null)
    {
        var decodedConversationId = Uri.UnescapeDataString(conversationId);

        _logger.LogInformation(
            "Received bot removal notification for conversation {ConversationId}, removed by {RemovedBy}",
            decodedConversationId, request?.RemovedBy ?? "unknown");

        // Find the mapping by conversation ID
        var mapping = await _dbContext.ExternalChannelMappings
            .FirstOrDefaultAsync(m =>
                m.Platform == ExternalPlatform.Teams &&
                m.ExternalGroupId == decodedConversationId);

        if (mapping == null)
        {
            _logger.LogDebug(
                "No mapping found for conversation {ConversationId} during bot removal",
                decodedConversationId);
            return NotFound(new { error = "No mapping found for this conversation" });
        }

        // Deactivate the mapping
        mapping.IsActive = false;
        mapping.ConversationReferenceJson = null; // Clear the reference since bot is removed
        mapping.LastModifiedBy = "TeamsBot:BotRemoved";
        mapping.LastModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Deactivated Teams mapping {MappingId} (conversation: {ConversationId}) due to bot removal. " +
            "Removed by: {RemovedBy}",
            mapping.Id, decodedConversationId, request?.RemovedBy ?? "unknown");

        return Ok(new
        {
            message = "Mapping deactivated due to bot removal",
            mappingId = mapping.Id,
            conversationId = decodedConversationId
        });
    }

    /// <summary>
    /// Create a new COBRA channel from an external platform (Teams or GroupMe).
    /// The channel will be created in the linked event and associated with the connector.
    /// </summary>
    [HttpPost("mappings/{mappingId:guid}/create-channel")]
    [ProducesResponseType(typeof(CreateChannelFromBotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateChannelFromBot(
        Guid mappingId,
        [FromBody] CreateChannelFromBotRequest request)
    {
        // Find the connector mapping
        var mapping = await _dbContext.ExternalChannelMappings
            .FirstOrDefaultAsync(m => m.Id == mappingId && m.IsActive);

        if (mapping == null)
        {
            return NotFound(new { error = "Connector not found or inactive" });
        }

        // Connector must be linked to an event
        if (!mapping.EventId.HasValue)
        {
            return BadRequest(new { error = "Connector is not linked to an event. Use 'link' command first." });
        }

        // Validate the event exists
        var eventEntity = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == mapping.EventId.Value);
        if (eventEntity == null)
        {
            return BadRequest(new { error = "Linked event not found" });
        }

        // Check if a channel with this name already exists in the event
        var existingChannel = await _dbContext.ChatThreads
            .FirstOrDefaultAsync(ct =>
                ct.EventId == mapping.EventId.Value &&
                ct.IsActive &&
                ct.Name.ToLower() == request.ChannelName.ToLower());

        if (existingChannel != null)
        {
            // Channel exists - link it to this connector instead of creating a new one
            _logger.LogInformation(
                "Channel '{ChannelName}' already exists in event {EventId} (ID: {ExistingChannelId}). Linking to connector {MappingId}.",
                existingChannel.Name, mapping.EventId, existingChannel.Id, mappingId);

            // Check if already linked to a different connector
            if (existingChannel.ExternalChannelMappingId.HasValue &&
                existingChannel.ExternalChannelMappingId.Value != mappingId)
            {
                _logger.LogWarning(
                    "Channel '{ChannelName}' is already linked to a different connector {ExistingMappingId}",
                    existingChannel.Name, existingChannel.ExternalChannelMappingId);

                return BadRequest(new
                {
                    error = $"Channel '{existingChannel.Name}' is already linked to a different external connector.",
                    existingChannelId = existingChannel.Id
                });
            }

            // Link existing channel to this connector
            existingChannel.ExternalChannelMappingId = mappingId;
            existingChannel.ChannelType = ChannelType.External;
            existingChannel.LastModifiedBy = request.CreatedByName ?? "TeamsBot";
            existingChannel.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return Ok(new CreateChannelFromBotResponse
            {
                Success = true,
                ChannelId = existingChannel.Id,
                ChannelName = existingChannel.Name,
                EventId = mapping.EventId.Value,
                EventName = eventEntity.Name,
                Message = $"Existing channel '{existingChannel.Name}' linked to this connector",
                LinkedMappingId = mappingId,
                WasExisting = true
            });
        }

        _logger.LogInformation(
            "Creating channel '{ChannelName}' from {Platform} connector {MappingId} for event {EventId}",
            request.ChannelName, mapping.Platform, mappingId, mapping.EventId);

        try
        {
            // Create the channel using ChannelService
            // Auto-link to the connector by setting ExternalChannelMappingId
            var createRequest = new CreateChannelRequest
            {
                EventId = mapping.EventId.Value,
                Name = request.ChannelName,
                Description = request.Description ?? $"Created from {mapping.Platform} by {request.CreatedByName ?? "bot user"}",
                ChannelType = ChannelType.External, // Will be set to External due to ExternalChannelMappingId
                IconName = request.IconName ?? GetDefaultIconForPlatform(mapping.Platform),
                ExternalChannelMappingId = mappingId // Auto-link to the connector
            };

            var channel = await _channelService.CreateChannelAsync(createRequest);

            _logger.LogInformation(
                "Created channel {ChannelId} '{ChannelName}' for event {EventId} from {Platform}, linked to connector {MappingId}",
                channel.Id, channel.Name, mapping.EventId, mapping.Platform, mappingId);

            return Ok(new CreateChannelFromBotResponse
            {
                Success = true,
                ChannelId = channel.Id,
                ChannelName = channel.Name,
                EventId = mapping.EventId.Value,
                EventName = eventEntity.Name,
                Message = $"Channel '{channel.Name}' created and linked to this connector",
                LinkedMappingId = mappingId,
                WasExisting = false
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create channel from bot for mapping {MappingId}", mappingId);
            return BadRequest(new { error = $"Failed to create channel: {ex.Message}" });
        }
    }

    /// <summary>
    /// Gets the default icon for a platform.
    /// </summary>
    private static string GetDefaultIconForPlatform(ExternalPlatform platform)
    {
        return platform switch
        {
            ExternalPlatform.Teams => "microsoft",
            ExternalPlatform.GroupMe => "comment-dots",
            _ => "comments"
        };
    }
}

/// <summary>
/// Request to create a channel from a bot command.
/// </summary>
public class CreateChannelFromBotRequest
{
    /// <summary>
    /// The name of the channel to create.
    /// </summary>
    public required string ChannelName { get; set; }

    /// <summary>
    /// Optional description for the channel.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional icon name (FontAwesome).
    /// </summary>
    public string? IconName { get; set; }

    /// <summary>
    /// Name of the user who requested the channel creation.
    /// </summary>
    public string? CreatedByName { get; set; }
}

/// <summary>
/// Response from channel creation.
/// </summary>
public class CreateChannelFromBotResponse
{
    public bool Success { get; set; }
    public Guid ChannelId { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public Guid EventId { get; set; }
    public string? EventName { get; set; }
    public string? Message { get; set; }

    /// <summary>
    /// The external channel mapping ID the channel is linked to.
    /// This enables bidirectional messaging between the COBRA channel and the external platform.
    /// </summary>
    public Guid? LinkedMappingId { get; set; }

    /// <summary>
    /// True if an existing channel was linked (rather than a new channel being created).
    /// </summary>
    public bool WasExisting { get; set; }
}

/// <summary>
/// Request body for bot removal notification.
/// </summary>
public class BotRemovedRequest
{
    /// <summary>
    /// Optional name of the user who removed the bot.
    /// </summary>
    public string? RemovedBy { get; set; }
}

/// <summary>
/// Request to broadcast an announcement to Teams.
/// </summary>
public class AnnouncementBroadcastRequest
{
    /// <summary>
    /// The event ID to broadcast to.
    /// </summary>
    public Guid EventId { get; set; }

    /// <summary>
    /// The announcement title (displayed in bold).
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// The announcement message content.
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Optional sender name override. If not provided, uses authenticated user's name.
    /// </summary>
    public string? SenderName { get; set; }

    /// <summary>
    /// Priority level: "normal", "high", or "urgent".
    /// Default is "normal".
    /// </summary>
    public string? Priority { get; set; }
}

/// <summary>
/// Response from announcement broadcast.
/// </summary>
public class AnnouncementBroadcastResponse
{
    /// <summary>
    /// Whether any channels received the announcement.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Number of Teams channels the announcement was sent to.
    /// </summary>
    public int ChannelsSent { get; set; }

    /// <summary>
    /// Human-readable status message.
    /// </summary>
    public required string Message { get; set; }
}
