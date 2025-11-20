-- ==========================================
-- Seed Data: Operational Periods
-- ==========================================
-- Creates test operational periods for POC testing
-- Allows testing of period grouping in frontend
--
-- USAGE: Run this AFTER applying migrations and base seed data
-- ==========================================

USE ChecklistPOC;
GO

-- Declare variables for consistent test data
DECLARE @EventId NVARCHAR(50) = 'INCIDENT-2025-001';
DECLARE @CreatedBy NVARCHAR(255) = 'System Seed';
DECLARE @UtcNow DATETIME2 = GETUTCDATE();

-- Clear existing test operational periods (if running multiple times)
DELETE FROM OperationalPeriods WHERE CreatedBy = 'System Seed';
GO

-- ==========================================
-- Operational Period 1: Morning Shift (completed)
-- ==========================================
DECLARE @OP1_Id UNIQUEIDENTIFIER = NEWID();
DECLARE @OP1_StartTime DATETIME2 = DATEADD(day, -1, DATEADD(hour, 6, CAST(CAST(GETUTCDATE() AS DATE) AS DATETIME2)));
DECLARE @OP1_EndTime DATETIME2 = DATEADD(day, -1, DATEADD(hour, 18, CAST(CAST(GETUTCDATE() AS DATE) AS DATETIME2)));

INSERT INTO OperationalPeriods (
    Id, EventId, Name, StartTime, EndTime, IsCurrent, Description,
    IsArchived, ArchivedBy, ArchivedAt, CreatedBy, CreatedAt
)
VALUES (
    @OP1_Id,
    'INCIDENT-2025-001',
    'OP 1 - Morning Shift (0600-1800)',
    @OP1_StartTime,
    @OP1_EndTime,
    0, -- Not current
    'First operational period - morning shift during initial response',
    0, -- Not archived
    NULL,
    NULL,
    'System Seed',
    GETUTCDATE()
);

PRINT 'Created OP 1 - Morning Shift (Past)';
GO

-- ==========================================
-- Operational Period 2: Night Shift (completed)
-- ==========================================
DECLARE @OP2_Id UNIQUEIDENTIFIER = NEWID();
DECLARE @OP2_StartTime DATETIME2 = DATEADD(day, -1, DATEADD(hour, 18, CAST(CAST(GETUTCDATE() AS DATE) AS DATETIME2)));
DECLARE @OP2_EndTime DATETIME2 = DATEADD(hour, 6, CAST(CAST(GETUTCDATE() AS DATE) AS DATETIME2));

INSERT INTO OperationalPeriods (
    Id, EventId, Name, StartTime, EndTime, IsCurrent, Description,
    IsArchived, ArchivedBy, ArchivedAt, CreatedBy, CreatedAt
)
VALUES (
    @OP2_Id,
    'INCIDENT-2025-001',
    'OP 2 - Night Shift (1800-0600)',
    @OP2_StartTime,
    @OP2_EndTime,
    0, -- Not current
    'Second operational period - night shift',
    0, -- Not archived
    NULL,
    NULL,
    'System Seed',
    GETUTCDATE()
);

PRINT 'Created OP 2 - Night Shift (Past)';
GO

-- ==========================================
-- Operational Period 3: Current Morning Shift (ACTIVE)
-- ==========================================
DECLARE @OP3_Id UNIQUEIDENTIFIER = NEWID();
DECLARE @OP3_StartTime DATETIME2 = DATEADD(hour, 6, CAST(CAST(GETUTCDATE() AS DATE) AS DATETIME2));

INSERT INTO OperationalPeriods (
    Id, EventId, Name, StartTime, EndTime, IsCurrent, Description,
    IsArchived, ArchivedBy, ArchivedAt, CreatedBy, CreatedAt
)
VALUES (
    @OP3_Id,
    'INCIDENT-2025-001',
    'OP 3 - Current Shift (0600-1800)',
    @OP3_StartTime,
    NULL, -- Still active, no end time
    1, -- CURRENT PERIOD
    'Active operational period - focus here',
    0, -- Not archived
    NULL,
    NULL,
    'System Seed',
    GETUTCDATE()
);

PRINT 'Created OP 3 - Current Shift (ACTIVE)';
GO

-- ==========================================
-- Associate existing checklists with operational periods
-- ==========================================
-- This simulates checklists created during different operational periods

-- Update a few checklists to belong to OP 1 (past)
UPDATE TOP (2) ChecklistInstances
SET
    OperationalPeriodId = (SELECT TOP 1 Id FROM OperationalPeriods WHERE Name LIKE '%OP 1%'),
    OperationalPeriodName = 'OP 1 - Morning Shift (0600-1800)'
WHERE OperationalPeriodId IS NULL
  AND EventId = 'INCIDENT-2025-001';

PRINT 'Associated 2 checklists with OP 1';

-- Update a few checklists to belong to OP 2 (past)
UPDATE TOP (2) ChecklistInstances
SET
    OperationalPeriodId = (SELECT TOP 1 Id FROM OperationalPeriods WHERE Name LIKE '%OP 2%'),
    OperationalPeriodName = 'OP 2 - Night Shift (1800-0600)'
WHERE OperationalPeriodId IS NULL
  AND EventId = 'INCIDENT-2025-001';

PRINT 'Associated 2 checklists with OP 2';

-- Update a few checklists to belong to OP 3 (current)
UPDATE TOP (3) ChecklistInstances
SET
    OperationalPeriodId = (SELECT TOP 1 Id FROM OperationalPeriods WHERE Name LIKE '%OP 3%'),
    OperationalPeriodName = 'OP 3 - Current Shift (0600-1800)'
WHERE OperationalPeriodId IS NULL
  AND EventId = 'INCIDENT-2025-001';

PRINT 'Associated 3 checklists with OP 3 (current)';

-- Leave remaining checklists with NULL OperationalPeriodId (incident-level)
PRINT 'Remaining checklists are incident-level (no specific operational period)';

-- ==========================================
-- Summary Report
-- ==========================================
SELECT
    'Operational Periods' AS [Type],
    COUNT(*) AS [Count]
FROM OperationalPeriods
WHERE CreatedBy = 'System Seed'

UNION ALL

SELECT
    'Checklists in OP 1' AS [Type],
    COUNT(*) AS [Count]
FROM ChecklistInstances
WHERE OperationalPeriodName LIKE '%OP 1%'

UNION ALL

SELECT
    'Checklists in OP 2' AS [Type],
    COUNT(*) AS [Count]
FROM ChecklistInstances
WHERE OperationalPeriodName LIKE '%OP 2%'

UNION ALL

SELECT
    'Checklists in OP 3 (Current)' AS [Type],
    COUNT(*) AS [Count]
FROM ChecklistInstances
WHERE OperationalPeriodName LIKE '%OP 3%'

UNION ALL

SELECT
    'Incident-Level Checklists' AS [Type],
    COUNT(*) AS [Count]
FROM ChecklistInstances
WHERE OperationalPeriodId IS NULL
  AND EventId = 'INCIDENT-2025-001';

GO

PRINT '';
PRINT '========================================';
PRINT 'Operational Periods Seed Data Complete!';
PRINT '========================================';
PRINT 'You can now test period grouping in the frontend.';
PRINT 'To switch current period, use the admin API endpoint:';
PRINT '  POST /api/operational-periods/{id}/set-current';
GO
