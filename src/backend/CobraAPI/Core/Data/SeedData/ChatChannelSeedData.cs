using Microsoft.EntityFrameworkCore;

namespace CobraAPI.Core.Data.SeedData;

/// <summary>
/// Seed data configuration for default chat channels.
/// Each event should have two default channels:
/// - Event Chat (Internal, default thread)
/// - Announcements
/// </summary>
public static class ChatChannelSeedData
{
    // Well-known event IDs from seed-events.sql
    private static readonly Guid DefaultEventId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid HurricaneEventId = Guid.Parse("22222222-2222-2222-2222-222222222002");
    private static readonly Guid EarthquakeEventId = Guid.Parse("22222222-2222-2222-2222-222222222003");
    private static readonly Guid TrainingEventId = Guid.Parse("22222222-2222-2222-2222-222222222004");
    private static readonly Guid WildfireEventId = Guid.Parse("22222222-2222-2222-2222-222222222005");

    // Well-known channel IDs for seed data (deterministic for idempotency)
    // Event Chat channels
    private static readonly Guid DefaultEventChatId = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000001");
    private static readonly Guid HurricaneEventChatId = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000002");
    private static readonly Guid EarthquakeEventChatId = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000003");
    private static readonly Guid TrainingEventChatId = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000004");
    private static readonly Guid WildfireEventChatId = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000005");

    // Announcements channels
    private static readonly Guid DefaultAnnouncementsId = Guid.Parse("aaaaaaaa-0002-0002-0002-000000000001");
    private static readonly Guid HurricaneAnnouncementsId = Guid.Parse("aaaaaaaa-0002-0002-0002-000000000002");
    private static readonly Guid EarthquakeAnnouncementsId = Guid.Parse("aaaaaaaa-0002-0002-0002-000000000003");
    private static readonly Guid TrainingAnnouncementsId = Guid.Parse("aaaaaaaa-0002-0002-0002-000000000004");
    private static readonly Guid WildfireAnnouncementsId = Guid.Parse("aaaaaaaa-0002-0002-0002-000000000005");

    /// <summary>
    /// Applies seed data for chat channels to the model.
    /// </summary>
    public static void Apply(ModelBuilder modelBuilder)
    {
        var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        const string systemUser = "system@cobra.mil";

        // Seed default chat channels for each event
        modelBuilder.Entity<ChatThread>().HasData(
            // POC Demo Event
            CreateEventChatChannel(DefaultEventChatId, DefaultEventId, systemUser, seedDate),
            CreateAnnouncementsChannel(DefaultAnnouncementsId, DefaultEventId, systemUser, seedDate),

            // Hurricane Milton Response
            CreateEventChatChannel(HurricaneEventChatId, HurricaneEventId, systemUser, seedDate),
            CreateAnnouncementsChannel(HurricaneAnnouncementsId, HurricaneEventId, systemUser, seedDate),

            // Earthquake Response (archived event - still gets channels)
            CreateEventChatChannel(EarthquakeEventChatId, EarthquakeEventId, systemUser, seedDate),
            CreateAnnouncementsChannel(EarthquakeAnnouncementsId, EarthquakeEventId, systemUser, seedDate),

            // EOC Full-Scale Exercise
            CreateEventChatChannel(TrainingEventChatId, TrainingEventId, systemUser, seedDate),
            CreateAnnouncementsChannel(TrainingAnnouncementsId, TrainingEventId, systemUser, seedDate),

            // Wildfire - Northern Region
            CreateEventChatChannel(WildfireEventChatId, WildfireEventId, systemUser, seedDate),
            CreateAnnouncementsChannel(WildfireAnnouncementsId, WildfireEventId, systemUser, seedDate)
        );
    }

    private static ChatThread CreateEventChatChannel(Guid id, Guid eventId, string createdBy, DateTime createdAt)
    {
        return new ChatThread
        {
            Id = id,
            EventId = eventId,
            Name = "Event Chat",
            Description = "General event discussion for all participants",
            ChannelType = ChannelType.Internal,
            DisplayOrder = 0,
            IconName = "comments",
            IsDefaultEventThread = true,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = createdAt
        };
    }

    private static ChatThread CreateAnnouncementsChannel(Guid id, Guid eventId, string createdBy, DateTime createdAt)
    {
        return new ChatThread
        {
            Id = id,
            EventId = eventId,
            Name = "Announcements",
            Description = "Important announcements from event leadership",
            ChannelType = ChannelType.Announcements,
            DisplayOrder = 1,
            IconName = "bullhorn",
            IsDefaultEventThread = false,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = createdAt
        };
    }
}
