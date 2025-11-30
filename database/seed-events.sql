-- ============================================================================
-- Checklist POC - Seed Data for Events and Event Categories
-- ============================================================================
-- Creates base event categories and sample events needed for checklists.
--
-- IMPORTANT: Run this file FIRST before seed-checklists.sql and
-- seed-operational-periods.sql
-- ============================================================================

USE ChecklistPOC;
GO

-- Clear existing event data if re-running
DELETE FROM Events;
DELETE FROM EventCategories;
GO

-- ============================================================================
-- Event Categories
-- ============================================================================
-- Based on FEMA Incident Types and ICS/NIMS categorization
-- Reference: FEMA National Risk Index, NIMS Resource Typing

-- Unplanned: Natural Disasters - Weather
DECLARE @HurricaneId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111001';
DECLARE @TornadoId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111002';
DECLARE @FloodId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111003';
DECLARE @WinterId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111004';
DECLARE @WildfireId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111005';
DECLARE @DroughtId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111006';
DECLARE @HeatId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111007';
DECLARE @ThunderstormId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111008';

-- Unplanned: Natural Disasters - Geologic
DECLARE @EarthquakeId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111010';
DECLARE @TsunamiId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111011';
DECLARE @VolcanoId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111012';
DECLARE @LandslideId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111013';

-- Unplanned: Technological/Industrial
DECLARE @HazmatId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111020';
DECLARE @InfraFailId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111021';
DECLARE @TransportId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111022';
DECLARE @NuclearId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111023';
DECLARE @CyberId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111024';
DECLARE @UtilityId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111025';

-- Unplanned: Human-Caused/Security
DECLARE @TerrorismId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111030';
DECLARE @CivilUnrestId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111031';
DECLARE @ActiveShooterId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111032';
DECLARE @SARId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111033';
DECLARE @MassCasualtyId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111034';

-- Unplanned: Public Health
DECLARE @PandemicId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111040';
DECLARE @DiseaseId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111041';
DECLARE @ContaminationId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111042';

-- Planned: Special Events
DECLARE @ParadeId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111050';
DECLARE @ConcertId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111051';
DECLARE @SportingId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111052';
DECLARE @FestivalId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111053';
DECLARE @VIPVisitId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111054';
DECLARE @ConferenceId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111055';

-- Planned: Exercises & Training
DECLARE @FullScaleId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111060';
DECLARE @FunctionalId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111061';
DECLARE @TabletopId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111062';
DECLARE @DrillId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111063';

INSERT INTO EventCategories (Id, Code, Name, EventType, SubGroup, DisplayOrder, IsActive, IconName)
VALUES
    -- ============================================================================
    -- UNPLANNED: Natural Disasters - Weather (FEMA Weather Hazards)
    -- ============================================================================
    (@HurricaneId, 'HURRICANE', 'Hurricane/Tropical Storm', 'Unplanned', 'Natural - Weather', 1, 1, 'hurricane'),
    (@TornadoId, 'TORNADO', 'Tornado/Severe Winds', 'Unplanned', 'Natural - Weather', 2, 1, 'tornado'),
    (@FloodId, 'FLOOD', 'Flood/Flash Flood', 'Unplanned', 'Natural - Weather', 3, 1, 'water'),
    (@WinterId, 'WINTER', 'Winter Storm/Ice/Snow', 'Unplanned', 'Natural - Weather', 4, 1, 'snowflake'),
    (@WildfireId, 'WILDFIRE', 'Wildfire/Brush Fire', 'Unplanned', 'Natural - Weather', 5, 1, 'fire'),
    (@DroughtId, 'DROUGHT', 'Drought', 'Unplanned', 'Natural - Weather', 6, 1, 'sun'),
    (@HeatId, 'HEAT', 'Extreme Heat', 'Unplanned', 'Natural - Weather', 7, 1, 'temperature-high'),
    (@ThunderstormId, 'TSTORM', 'Severe Thunderstorm/Hail', 'Unplanned', 'Natural - Weather', 8, 1, 'cloud-bolt'),

    -- ============================================================================
    -- UNPLANNED: Natural Disasters - Geologic (FEMA Geologic Hazards)
    -- ============================================================================
    (@EarthquakeId, 'EARTHQUAKE', 'Earthquake', 'Unplanned', 'Natural - Geologic', 10, 1, 'house-crack'),
    (@TsunamiId, 'TSUNAMI', 'Tsunami', 'Unplanned', 'Natural - Geologic', 11, 1, 'wave-pulse'),
    (@VolcanoId, 'VOLCANO', 'Volcanic Activity', 'Unplanned', 'Natural - Geologic', 12, 1, 'volcano'),
    (@LandslideId, 'LANDSLIDE', 'Landslide/Mudslide/Debris Flow', 'Unplanned', 'Natural - Geologic', 13, 1, 'mountain'),

    -- ============================================================================
    -- UNPLANNED: Technological/Industrial (FEMA Tech Hazards)
    -- ============================================================================
    (@HazmatId, 'HAZMAT', 'Hazardous Materials Release', 'Unplanned', 'Technological', 20, 1, 'biohazard'),
    (@InfraFailId, 'INFRA', 'Infrastructure Failure/Collapse', 'Unplanned', 'Technological', 21, 1, 'building'),
    (@TransportId, 'TRANSPORT', 'Transportation Incident', 'Unplanned', 'Technological', 22, 1, 'plane-slash'),
    (@NuclearId, 'NUCLEAR', 'Nuclear/Radiological Incident', 'Unplanned', 'Technological', 23, 1, 'radiation'),
    (@CyberId, 'CYBER', 'Cyber Attack/IT Infrastructure', 'Unplanned', 'Technological', 24, 1, 'shield-virus'),
    (@UtilityId, 'UTILITY', 'Utility Disruption (Power/Water/Gas)', 'Unplanned', 'Technological', 25, 1, 'plug-circle-exclamation'),

    -- ============================================================================
    -- UNPLANNED: Human-Caused/Security (FEMA Security Hazards)
    -- ============================================================================
    (@TerrorismId, 'TERRORISM', 'Terrorism/CBRNE', 'Unplanned', 'Human-Caused', 30, 1, 'triangle-exclamation'),
    (@CivilUnrestId, 'CIVIL', 'Civil Disturbance/Unrest', 'Unplanned', 'Human-Caused', 31, 1, 'users'),
    (@ActiveShooterId, 'SHOOTER', 'Active Shooter/Threat', 'Unplanned', 'Human-Caused', 32, 1, 'person-rifle'),
    (@SARId, 'SAR', 'Search and Rescue', 'Unplanned', 'Human-Caused', 33, 1, 'person-hiking'),
    (@MassCasualtyId, 'MCI', 'Mass Casualty Incident', 'Unplanned', 'Human-Caused', 34, 1, 'truck-medical'),

    -- ============================================================================
    -- UNPLANNED: Public Health (FEMA Health Hazards)
    -- ============================================================================
    (@PandemicId, 'PANDEMIC', 'Pandemic/Epidemic', 'Unplanned', 'Public Health', 40, 1, 'virus'),
    (@DiseaseId, 'DISEASE', 'Disease Outbreak', 'Unplanned', 'Public Health', 41, 1, 'disease'),
    (@ContaminationId, 'CONTAM', 'Food/Water Contamination', 'Unplanned', 'Public Health', 42, 1, 'flask-vial'),

    -- ============================================================================
    -- PLANNED: Special Events (ICS Planned Events)
    -- ============================================================================
    (@ParadeId, 'PARADE', 'Parade/March/Demonstration', 'Planned', 'Special Event', 50, 1, 'flag'),
    (@ConcertId, 'CONCERT', 'Concert/Performance', 'Planned', 'Special Event', 51, 1, 'music'),
    (@SportingId, 'SPORTS', 'Sporting Event', 'Planned', 'Special Event', 52, 1, 'football'),
    (@FestivalId, 'FESTIVAL', 'Festival/Fair/Carnival', 'Planned', 'Special Event', 53, 1, 'tent'),
    (@VIPVisitId, 'VIP', 'VIP/Dignitary Visit', 'Planned', 'Special Event', 54, 1, 'user-shield'),
    (@ConferenceId, 'CONF', 'Conference/Convention', 'Planned', 'Special Event', 55, 1, 'building-columns'),

    -- ============================================================================
    -- PLANNED: Exercises & Training (HSEEP Exercise Types)
    -- ============================================================================
    (@FullScaleId, 'FSE', 'Full-Scale Exercise', 'Planned', 'Exercise', 60, 1, 'person-running'),
    (@FunctionalId, 'FE', 'Functional Exercise', 'Planned', 'Exercise', 61, 1, 'gears'),
    (@TabletopId, 'TTX', 'Tabletop Exercise', 'Planned', 'Exercise', 62, 1, 'table'),
    (@DrillId, 'DRILL', 'Drill/Training', 'Planned', 'Exercise', 63, 1, 'graduation-cap');

PRINT 'Created 34 event categories (FEMA/ICS/NIMS based)';

-- ============================================================================
-- Events
-- ============================================================================
-- Sample events for POC demonstration

-- POC Demo Event - used by checklists and operational periods
DECLARE @PocEventId UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222001';
DECLARE @HurricaneEventId UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222002';
DECLARE @EarthquakeEventId UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222003';
DECLARE @TrainingEventId UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222004';

INSERT INTO Events (Id, Name, EventType, PrimaryCategoryId, AdditionalCategoryIds, IsActive, IsArchived, CreatedBy, CreatedAt)
VALUES
    -- Active Hurricane event (Unplanned) with additional categories
    (@HurricaneEventId,
     'Hurricane Milton Response - November 2025',
     'Unplanned',
     @HurricaneId,
     NULL,
     1, 0,
     'james.rodriguez@cobra.mil',
     DATEADD(DAY, -5, GETUTCDATE())),

    -- Training exercise event (Planned)
    (@TrainingEventId,
     'EOC Full-Scale Exercise 2025',
     'Planned',
     @FullScaleId,
     NULL,
     1, 0,
     'sarah.jackson@cobra.mil',
     DATEADD(DAY, -10, GETUTCDATE())),

    -- Archived earthquake event (Unplanned)
    (@EarthquakeEventId,
     'Earthquake Response - October 2025',
     'Unplanned',
     @EarthquakeId,
     NULL,
     0, 1,
     'maria.chen@cobra.mil',
     DATEADD(MONTH, -1, GETUTCDATE())),

    -- POC Demo Event (Unplanned - generic for testing)
    (@PocEventId,
     'POC Demo Event',
     'Unplanned',
     @HurricaneId,
     NULL,
     1, 0,
     'system@cobra.mil',
     GETUTCDATE());

PRINT 'Created 4 events';

-- ============================================================================
-- Output Event IDs for other seed scripts
-- ============================================================================
PRINT '';
PRINT '============================================================================';
PRINT 'Event Seed Data Summary';
PRINT '============================================================================';
PRINT '';
PRINT 'Use these Event IDs in other seed scripts:';
PRINT '  POC Demo Event:      ' + CAST(@PocEventId AS VARCHAR(50));
PRINT '  Hurricane Milton:    ' + CAST(@HurricaneEventId AS VARCHAR(50));
PRINT '  Earthquake:          ' + CAST(@EarthquakeEventId AS VARCHAR(50));
PRINT '  Training Exercise:   ' + CAST(@TrainingEventId AS VARCHAR(50));
PRINT '';

SELECT Id, Name, EventType, IsActive, IsArchived, CreatedBy
FROM Events
ORDER BY CreatedAt DESC;

PRINT '';
PRINT '============================================================================';
PRINT 'Event seed data loaded successfully!';
PRINT '============================================================================';

GO
