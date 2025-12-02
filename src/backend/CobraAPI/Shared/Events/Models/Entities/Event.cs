namespace CobraAPI.Shared.Events.Models.Entities;

/// <summary>
/// Event - represents an incident or planned event that checklists are associated with.
/// Events have a type (Planned/Unplanned) and categories based on FEMA/NIMS standards.
/// </summary>
public class Event
{
    public Guid Id { get; set; }

    /// <summary>
    /// Event name (e.g., "Hurricane Milton Response", "July 4th Parade Detail")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Event type: "PLANNED" or "UNPLANNED"
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Primary category for this event
    /// </summary>
    public Guid PrimaryCategoryId { get; set; }

    /// <summary>
    /// Additional category IDs (JSON array of GUIDs)
    /// Allows events to span multiple categories (e.g., Hurricane + Flood + Power Outage)
    /// </summary>
    public string? AdditionalCategoryIds { get; set; }

    /// <summary>
    /// Whether this event is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this event has been archived (soft delete)
    /// </summary>
    public bool IsArchived { get; set; } = false;

    public string? ArchivedBy { get; set; }
    public DateTime? ArchivedAt { get; set; }

    // Audit fields
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? LastModifiedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }

    // Navigation properties
    public EventCategory PrimaryCategory { get; set; } = null!;
}
