-- ============================================================================
-- Checklist POC - Seed Data for Default Chat Channels
-- ============================================================================
-- Creates default chat channels (Event Chat and Announcements) for all events
-- that don't already have them.
--
-- This script is idempotent - safe to run multiple times.
--
-- IMPORTANT: Run this file AFTER seed-events.sql
-- ============================================================================

USE COBRAPOC;
SET QUOTED_IDENTIFIER ON;
GO

-- ============================================================================
-- Create Default Channels for Events Without Them
-- ============================================================================
-- Default channels:
-- 1. Event Chat (Internal, DisplayOrder=0, IsDefaultEventThread=true)
-- 2. Announcements (Announcements, DisplayOrder=1)
--
-- ChannelType enum values:
--   Internal = 0
--   Announcements = 1
--   External = 2
--   Position = 3
--   Custom = 4
-- ============================================================================

DECLARE @Now DATETIME2 = GETUTCDATE();
DECLARE @SystemUser NVARCHAR(200) = 'system@cobra.mil';
DECLARE @EventsProcessed INT = 0;
DECLARE @ChannelsCreated INT = 0;

PRINT 'Starting default chat channel creation...';
PRINT '';

-- Create a temp table to track events needing channels
DECLARE @EventsNeedingChannels TABLE (
    EventId UNIQUEIDENTIFIER,
    EventName NVARCHAR(200)
);

-- Find events that don't have an Internal (Event Chat) channel
INSERT INTO @EventsNeedingChannels (EventId, EventName)
SELECT e.Id, e.Name
FROM Events e
WHERE NOT EXISTS (
    SELECT 1 FROM ChatThreads ct
    WHERE ct.EventId = e.Id
    AND ct.ChannelType = 0  -- Internal
    AND ct.IsDefaultEventThread = 1
);

-- Get count for logging
SELECT @EventsProcessed = COUNT(*) FROM @EventsNeedingChannels;

IF @EventsProcessed = 0
BEGIN
    PRINT 'All events already have default channels. Nothing to do.';
END
ELSE
BEGIN
    PRINT CONCAT('Found ', @EventsProcessed, ' event(s) needing default channels:');

    -- Log the events
    DECLARE @EventName NVARCHAR(200);
    DECLARE @EventId UNIQUEIDENTIFIER;

    DECLARE event_cursor CURSOR FOR
        SELECT EventId, EventName FROM @EventsNeedingChannels;

    OPEN event_cursor;
    FETCH NEXT FROM event_cursor INTO @EventId, @EventName;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        PRINT CONCAT('  - ', @EventName, ' (', @EventId, ')');
        FETCH NEXT FROM event_cursor INTO @EventId, @EventName;
    END

    CLOSE event_cursor;
    DEALLOCATE event_cursor;

    PRINT '';

    -- Create Event Chat channel for each event
    INSERT INTO ChatThreads (
        Id,
        EventId,
        Name,
        Description,
        ChannelType,
        DisplayOrder,
        IconName,
        IsDefaultEventThread,
        IsActive,
        CreatedBy,
        CreatedAt
    )
    SELECT
        NEWID(),
        enc.EventId,
        'Event Chat',
        'General event discussion for all participants',
        0,  -- ChannelType.Internal
        0,  -- DisplayOrder
        'comments',
        1,  -- IsDefaultEventThread = true
        1,  -- IsActive
        @SystemUser,
        @Now
    FROM @EventsNeedingChannels enc;

    SET @ChannelsCreated = @ChannelsCreated + @@ROWCOUNT;
    PRINT CONCAT('Created ', @@ROWCOUNT, ' Event Chat channel(s)');

    -- Create Announcements channel for each event (only if it doesn't exist)
    INSERT INTO ChatThreads (
        Id,
        EventId,
        Name,
        Description,
        ChannelType,
        DisplayOrder,
        IconName,
        IsDefaultEventThread,
        IsActive,
        CreatedBy,
        CreatedAt
    )
    SELECT
        NEWID(),
        enc.EventId,
        'Announcements',
        'Important announcements from event leadership',
        1,  -- ChannelType.Announcements
        1,  -- DisplayOrder
        'bullhorn',
        0,  -- IsDefaultEventThread = false
        1,  -- IsActive
        @SystemUser,
        @Now
    FROM @EventsNeedingChannels enc
    WHERE NOT EXISTS (
        SELECT 1 FROM ChatThreads ct
        WHERE ct.EventId = enc.EventId
        AND ct.ChannelType = 1  -- Announcements
    );

    SET @ChannelsCreated = @ChannelsCreated + @@ROWCOUNT;
    PRINT CONCAT('Created ', @@ROWCOUNT, ' Announcements channel(s)');
END

-- ============================================================================
-- Summary
-- ============================================================================
PRINT '';
PRINT '============================================================================';
PRINT 'Chat Channel Seed Data Summary';
PRINT '============================================================================';
PRINT '';

-- Show all channels per event
SELECT
    e.Name AS EventName,
    ct.Name AS ChannelName,
    CASE ct.ChannelType
        WHEN 0 THEN 'Internal'
        WHEN 1 THEN 'Announcements'
        WHEN 2 THEN 'External'
        WHEN 3 THEN 'Position'
        WHEN 4 THEN 'Custom'
    END AS ChannelType,
    ct.DisplayOrder,
    ct.IsDefaultEventThread,
    ct.IsActive
FROM Events e
LEFT JOIN ChatThreads ct ON ct.EventId = e.Id
WHERE e.IsArchived = 0
ORDER BY e.Name, ct.DisplayOrder;

PRINT '';
PRINT CONCAT('Total channels created: ', @ChannelsCreated);
PRINT '';
PRINT '============================================================================';
PRINT 'Chat channel seed data loaded successfully!';
PRINT '============================================================================';

GO
