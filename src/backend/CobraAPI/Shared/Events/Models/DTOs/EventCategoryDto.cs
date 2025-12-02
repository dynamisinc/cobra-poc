namespace CobraAPI.Shared.Events.Models.DTOs;

/// <summary>
/// EventCategoryDto - Data transfer object for event categories
///
/// Purpose:
///   Represents a standard event category based on FEMA/NIMS classifications.
///   Used for populating dropdowns and categorizing events.
///
/// Event Types:
///   - PLANNED: Scheduled events (parades, airshows, training exercises)
///   - UNPLANNED: Emergency incidents (hurricanes, wildfires, hazmat)
///
/// SubGroups:
///   - Special Event (PLANNED)
///   - Natural Disaster (UNPLANNED)
///   - Technological/Human-Caused (UNPLANNED)
///
/// Author: Checklist POC Team
/// Last Modified: 2025-11-29
/// </summary>
public record EventCategoryDto
{
    /// <summary>
    /// Unique identifier for this category
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Unique code for programmatic reference
    /// Example: "HURRICANE", "PARADE", "HAZMAT"
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Display name for UI
    /// Example: "Hurricane/Tropical Storm", "Parade/Procession"
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Event type: "PLANNED" or "UNPLANNED"
    /// </summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>
    /// Grouping for UI organization
    /// Example: "Natural Disaster", "Special Event", "Technological/Human-Caused"
    /// </summary>
    public string SubGroup { get; init; } = string.Empty;

    /// <summary>
    /// Order for display in dropdowns (lower = higher)
    /// </summary>
    public int DisplayOrder { get; init; }

    /// <summary>
    /// Whether this category is available for selection
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// FontAwesome icon name hint for UI
    /// Example: "hurricane", "fire", "parade"
    /// </summary>
    public string? IconName { get; init; }
}
