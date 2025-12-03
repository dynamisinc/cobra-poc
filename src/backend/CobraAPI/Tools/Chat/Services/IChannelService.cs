namespace CobraAPI.Tools.Chat.Services;

/// <summary>
/// Interface for channel management operations.
/// Channels represent different conversation spaces within an event.
/// </summary>
public interface IChannelService
{
    /// <summary>
    /// Gets all channels for an event.
    /// </summary>
    Task<List<ChatThreadDto>> GetEventChannelsAsync(Guid eventId);

    /// <summary>
    /// Gets a specific channel by ID.
    /// </summary>
    Task<ChatThreadDto?> GetChannelAsync(Guid channelId);

    /// <summary>
    /// Creates a new channel in an event.
    /// </summary>
    Task<ChatThreadDto> CreateChannelAsync(CreateChannelRequest request);

    /// <summary>
    /// Creates the default channels for a new event.
    /// Called during event creation.
    /// </summary>
    Task CreateDefaultChannelsAsync(Guid eventId, string createdBy);

    /// <summary>
    /// Updates channel metadata (name, description, icon, color).
    /// </summary>
    Task<ChatThreadDto?> UpdateChannelAsync(Guid channelId, UpdateChannelRequest request);

    /// <summary>
    /// Reorders channels within an event.
    /// </summary>
    Task ReorderChannelsAsync(Guid eventId, List<Guid> orderedChannelIds);

    /// <summary>
    /// Deletes a channel (soft delete).
    /// Cannot delete the default event channel or channels with External type.
    /// </summary>
    Task<bool> DeleteChannelAsync(Guid channelId);
}
