using Microsoft.EntityFrameworkCore;
using CobraAPI.Core.Data;
using CobraAPI.Core.Models;

namespace CobraAPI.Tools.Chat.Services;

/// <summary>
/// Service for managing chat channels within events.
/// Handles CRUD operations and default channel creation.
/// </summary>
public class ChannelService : IChannelService
{
    private readonly CobraDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ChannelService> _logger;

    public ChannelService(
        CobraDbContext dbContext,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ChannelService> logger)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    private UserContext GetUserContext()
    {
        return _httpContextAccessor.HttpContext?.Items["UserContext"] as UserContext
               ?? throw new UnauthorizedAccessException("User context not found");
    }

    /// <inheritdoc />
    public async Task<List<ChatThreadDto>> GetEventChannelsAsync(Guid eventId)
    {
        var channels = await _dbContext.ChatThreads
            .Include(ct => ct.ExternalChannelMapping)
            .Where(ct => ct.EventId == eventId && ct.IsActive)
            .OrderBy(ct => ct.DisplayOrder)
            .ThenBy(ct => ct.CreatedAt)
            .Select(ct => MapToDto(ct))
            .ToListAsync();

        return channels;
    }

    /// <inheritdoc />
    public async Task<ChatThreadDto?> GetChannelAsync(Guid channelId)
    {
        var channel = await _dbContext.ChatThreads
            .Include(ct => ct.ExternalChannelMapping)
            .Where(ct => ct.Id == channelId && ct.IsActive)
            .Select(ct => MapToDto(ct))
            .FirstOrDefaultAsync();

        return channel;
    }

    /// <inheritdoc />
    public async Task<ChatThreadDto> CreateChannelAsync(CreateChannelRequest request)
    {
        var userContext = GetUserContext();
        var now = DateTime.UtcNow;

        // Get max display order for this event
        var maxOrder = await _dbContext.ChatThreads
            .Where(ct => ct.EventId == request.EventId && ct.IsActive)
            .MaxAsync(ct => (int?)ct.DisplayOrder) ?? -1;

        var channel = new ChatThread
        {
            Id = Guid.NewGuid(),
            EventId = request.EventId,
            Name = request.Name,
            Description = request.Description,
            ChannelType = request.ChannelType,
            DisplayOrder = maxOrder + 1,
            IconName = request.IconName,
            Color = request.Color,
            IsDefaultEventThread = false,
            IsActive = true,
            CreatedBy = userContext.Email,
            CreatedAt = now
        };

        _dbContext.ChatThreads.Add(channel);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Created channel {ChannelId} '{Name}' of type {Type} for event {EventId}",
            channel.Id, channel.Name, channel.ChannelType, request.EventId);

        return MapToDto(channel);
    }

    /// <inheritdoc />
    public async Task CreateDefaultChannelsAsync(Guid eventId, string createdBy)
    {
        var now = DateTime.UtcNow;

        var defaultChannels = new List<ChatThread>
        {
            new ChatThread
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Name = "Event Chat",
                Description = "General event discussion for all participants",
                ChannelType = ChannelType.Internal,
                DisplayOrder = 0,
                IconName = "comments",
                IsDefaultEventThread = true,
                IsActive = true,
                CreatedBy = createdBy,
                CreatedAt = now
            },
            new ChatThread
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Name = "Announcements",
                Description = "Important announcements from event leadership",
                ChannelType = ChannelType.Announcements,
                DisplayOrder = 1,
                IconName = "bullhorn",
                IsDefaultEventThread = false,
                IsActive = true,
                CreatedBy = createdBy,
                CreatedAt = now
            }
        };

        _dbContext.ChatThreads.AddRange(defaultChannels);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Created {Count} default channels for event {EventId}",
            defaultChannels.Count, eventId);
    }

    /// <inheritdoc />
    public async Task<ChatThreadDto?> UpdateChannelAsync(Guid channelId, UpdateChannelRequest request)
    {
        var channel = await _dbContext.ChatThreads
            .Include(ct => ct.ExternalChannelMapping)
            .FirstOrDefaultAsync(ct => ct.Id == channelId && ct.IsActive);

        if (channel == null)
        {
            return null;
        }

        // Update only provided fields
        if (request.Name != null)
        {
            channel.Name = request.Name;
        }

        if (request.Description != null)
        {
            channel.Description = request.Description;
        }

        if (request.IconName != null)
        {
            channel.IconName = request.IconName;
        }

        if (request.Color != null)
        {
            channel.Color = request.Color;
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Updated channel {ChannelId}", channelId);

        return MapToDto(channel);
    }

    /// <inheritdoc />
    public async Task ReorderChannelsAsync(Guid eventId, List<Guid> orderedChannelIds)
    {
        var channels = await _dbContext.ChatThreads
            .Where(ct => ct.EventId == eventId && ct.IsActive)
            .ToListAsync();

        for (int i = 0; i < orderedChannelIds.Count; i++)
        {
            var channel = channels.FirstOrDefault(c => c.Id == orderedChannelIds[i]);
            if (channel != null)
            {
                channel.DisplayOrder = i;
            }
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Reordered {Count} channels for event {EventId}",
            orderedChannelIds.Count, eventId);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteChannelAsync(Guid channelId)
    {
        var channel = await _dbContext.ChatThreads
            .FirstOrDefaultAsync(ct => ct.Id == channelId && ct.IsActive);

        if (channel == null)
        {
            return false;
        }

        // Cannot delete default event channel
        if (channel.IsDefaultEventThread)
        {
            _logger.LogWarning(
                "Cannot delete default event channel {ChannelId}",
                channelId);
            return false;
        }

        // Cannot delete external channels (should be deactivated via external channel mapping)
        if (channel.ChannelType == ChannelType.External)
        {
            _logger.LogWarning(
                "Cannot delete external channel {ChannelId}. Deactivate the external mapping instead.",
                channelId);
            return false;
        }

        // Soft delete
        channel.IsActive = false;
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Deleted (soft) channel {ChannelId}", channelId);

        return true;
    }

    /// <summary>
    /// Maps a ChatThread entity to ChatThreadDto.
    /// </summary>
    private static ChatThreadDto MapToDto(ChatThread ct)
    {
        return new ChatThreadDto
        {
            Id = ct.Id,
            EventId = ct.EventId,
            Name = ct.Name,
            Description = ct.Description,
            ChannelType = ct.ChannelType,
            ChannelTypeName = ct.ChannelType.ToString(),
            IsDefaultEventThread = ct.IsDefaultEventThread,
            DisplayOrder = ct.DisplayOrder,
            IconName = ct.IconName,
            Color = ct.Color,
            MessageCount = ct.Messages?.Count(m => m.IsActive) ?? 0,
            CreatedAt = ct.CreatedAt,
            ExternalChannel = ct.ExternalChannelMapping != null
                ? new ExternalChannelMappingDto
                {
                    Id = ct.ExternalChannelMapping.Id,
                    EventId = ct.ExternalChannelMapping.EventId,
                    Platform = ct.ExternalChannelMapping.Platform,
                    PlatformName = ct.ExternalChannelMapping.Platform.ToString(),
                    ExternalGroupId = ct.ExternalChannelMapping.ExternalGroupId,
                    ExternalGroupName = ct.ExternalChannelMapping.ExternalGroupName,
                    ShareUrl = ct.ExternalChannelMapping.ShareUrl,
                    IsActive = ct.ExternalChannelMapping.IsActive,
                    CreatedAt = ct.ExternalChannelMapping.CreatedAt
                }
                : null
        };
    }
}
