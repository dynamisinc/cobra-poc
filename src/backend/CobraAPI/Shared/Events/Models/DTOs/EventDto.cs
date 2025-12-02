namespace CobraAPI.Shared.Events.Models.DTOs;

/// <summary>
/// EventDto - Data transfer object for events (incidents/operations)
///
/// Purpose:
///   Represents an event (incident or planned operation) that checklists,
///   operational periods, and other POC tools are associated with.
///
/// Event Types:
///   - PLANNED: Scheduled events (parades, conferences, training)
///   - UNPLANNED: Emergency incidents (hurricanes, fires, hazmat)
///
/// Author: Checklist POC Team
/// Last Modified: 2025-11-29
/// </summary>
public record EventDto
{
    /// <summary>
    /// Unique identifier for this event
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Event name for display
    /// Example: "Hurricane Milton Response", "July 4th Parade Detail"
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Event type: "PLANNED" or "UNPLANNED"
    /// </summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>
    /// Primary category ID (FK to EventCategory)
    /// </summary>
    public Guid PrimaryCategoryId { get; init; }

    /// <summary>
    /// Primary category details (included for convenience)
    /// </summary>
    public EventCategoryDto? PrimaryCategory { get; init; }

    /// <summary>
    /// Additional category IDs (JSON array of GUIDs)
    /// For events that span multiple categories
    /// Example: Hurricane + Flood + Power Outage
    /// </summary>
    public List<Guid>? AdditionalCategoryIds { get; init; }

    /// <summary>
    /// Whether this event is currently active
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Whether this event has been archived
    /// </summary>
    public bool IsArchived { get; init; }

    /// <summary>
    /// User who created this event
    /// </summary>
    public string CreatedBy { get; init; } = string.Empty;

    /// <summary>
    /// When this event was created (UTC)
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// User who last modified this event
    /// </summary>
    public string? LastModifiedBy { get; init; }

    /// <summary>
    /// When this event was last modified (UTC)
    /// </summary>
    public DateTime? LastModifiedAt { get; init; }
}
