using Microsoft.AspNetCore.Mvc;

namespace CobraAPI.Tools.Chat.Controllers;

/// <summary>
/// API controller for chat operations.
/// Provides endpoints for sending messages, retrieving messages, and managing channels.
/// </summary>
[ApiController]
[Route("api/events/{eventId:guid}/chat")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IChannelService _channelService;
    private readonly IExternalMessagingService _externalMessagingService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        IChatService chatService,
        IChannelService channelService,
        IExternalMessagingService externalMessagingService,
        ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _channelService = channelService;
        _externalMessagingService = externalMessagingService;
        _logger = logger;
    }

    #region Channels

    /// <summary>
    /// Gets all channels for an event.
    /// </summary>
    [HttpGet("channels")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ChatThreadDto>>> GetChannels(Guid eventId)
    {
        var channels = await _channelService.GetEventChannelsAsync(eventId);
        return Ok(channels);
    }

    /// <summary>
    /// Gets a specific channel by ID.
    /// </summary>
    [HttpGet("channels/{channelId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChatThreadDto>> GetChannel(Guid eventId, Guid channelId)
    {
        var channel = await _channelService.GetChannelAsync(channelId);
        if (channel == null)
        {
            return NotFound("Channel not found");
        }
        return Ok(channel);
    }

    /// <summary>
    /// Creates a new channel in an event.
    /// </summary>
    [HttpPost("channels")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChatThreadDto>> CreateChannel(
        Guid eventId,
        [FromBody] CreateChannelRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Channel name is required");
        }

        request.EventId = eventId;
        var channel = await _channelService.CreateChannelAsync(request);

        return CreatedAtAction(
            nameof(GetChannel),
            new { eventId, channelId = channel.Id },
            channel);
    }

    /// <summary>
    /// Updates a channel's metadata.
    /// </summary>
    [HttpPatch("channels/{channelId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChatThreadDto>> UpdateChannel(
        Guid eventId,
        Guid channelId,
        [FromBody] UpdateChannelRequest request)
    {
        var channel = await _channelService.UpdateChannelAsync(channelId, request);
        if (channel == null)
        {
            return NotFound("Channel not found");
        }
        return Ok(channel);
    }

    /// <summary>
    /// Reorders channels within an event.
    /// </summary>
    [HttpPut("channels/reorder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> ReorderChannels(
        Guid eventId,
        [FromBody] List<Guid> orderedChannelIds)
    {
        await _channelService.ReorderChannelsAsync(eventId, orderedChannelIds);
        return NoContent();
    }

    /// <summary>
    /// Deletes a channel (soft delete).
    /// Cannot delete the default event channel or external channels.
    /// </summary>
    [HttpDelete("channels/{channelId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteChannel(Guid eventId, Guid channelId)
    {
        var result = await _channelService.DeleteChannelAsync(channelId);
        if (!result)
        {
            return BadRequest("Cannot delete this channel. It may be a default or external channel.");
        }
        return NoContent();
    }

    #endregion

    /// <summary>
    /// Gets or creates the default chat thread for an event.
    /// </summary>
    [HttpGet("thread")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ChatThreadDto>> GetEventChatThread(Guid eventId)
    {
        var thread = await _chatService.GetOrCreateEventChatThreadAsync(eventId);
        return Ok(thread);
    }

    /// <summary>
    /// Gets messages for a chat thread with optional pagination.
    /// </summary>
    /// <param name="eventId">The event ID</param>
    /// <param name="threadId">The chat thread ID</param>
    /// <param name="skip">Number of messages to skip</param>
    /// <param name="take">Number of messages to return</param>
    [HttpGet("thread/{threadId:guid}/messages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ChatMessageDto>>> GetMessages(
        Guid eventId,
        Guid threadId,
        [FromQuery] int? skip = null,
        [FromQuery] int? take = null)
    {
        var messages = await _chatService.GetMessagesAsync(threadId, skip, take);
        return Ok(messages);
    }

    /// <summary>
    /// Sends a new chat message.
    /// </summary>
    [HttpPost("thread/{threadId:guid}/messages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChatMessageDto>> SendMessage(
        Guid eventId,
        Guid threadId,
        [FromBody] SendMessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest("Message cannot be empty");
        }

        var message = await _chatService.SendMessageAsync(eventId, threadId, request.Message);
        return Ok(message);
    }

    /// <summary>
    /// Gets external channel mappings for an event.
    /// </summary>
    [HttpGet("external-channels")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ExternalChannelMappingDto>>> GetExternalChannels(Guid eventId)
    {
        var channels = await _externalMessagingService.GetEventChannelMappingsAsync(eventId);
        return Ok(channels);
    }

    /// <summary>
    /// Creates a new external channel mapping (connects GroupMe group to event).
    /// </summary>
    [HttpPost("external-channels")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ExternalChannelMappingDto>> CreateExternalChannel(
        Guid eventId,
        [FromBody] CreateExternalChannelApiRequest request)
    {
        try
        {
            var mapping = await _externalMessagingService.CreateExternalChannelAsync(new CreateExternalChannelRequest
            {
                EventId = eventId,
                Platform = request.Platform,
                CustomGroupName = request.CustomGroupName
            });

            return CreatedAtAction(
                nameof(GetExternalChannels),
                new { eventId },
                mapping);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to create external channel for event {EventId}", eventId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deactivates an external channel mapping.
    /// </summary>
    [HttpDelete("external-channels/{mappingId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeactivateExternalChannel(
        Guid eventId,
        Guid mappingId,
        [FromQuery] bool archiveExternalGroup = false)
    {
        try
        {
            await _externalMessagingService.DeactivateChannelAsync(mappingId, archiveExternalGroup);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Channel mapping not found");
        }
    }
}

/// <summary>
/// Request model for creating an external channel.
/// </summary>
public class CreateExternalChannelApiRequest
{
    public ExternalPlatform Platform { get; set; }
    public string? CustomGroupName { get; set; }
}
