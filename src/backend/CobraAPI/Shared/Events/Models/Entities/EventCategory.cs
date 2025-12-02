namespace CobraAPI.Shared.Events.Models.Entities;

/// <summary>
/// Event Category - defines categories for events based on FEMA/NIMS standards.
/// Categories are driven by EventType (Planned vs Unplanned).
/// Used for template auto-creation suggestions and cross-tool categorization.
/// </summary>
public class EventCategory
{
    public Guid Id { get; set; }

    /// <summary>
    /// Unique code for the category (e.g., "HURRICANE", "PARADE")
    /// Used for programmatic matching and API references
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display name (e.g., "Hurricane/Tropical Storm", "Parade/Procession")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Event type this category belongs to: "PLANNED" or "UNPLANNED"
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Grouping for UI organization (e.g., "Natural Disaster", "Technological", "Special Event")
    /// </summary>
    public string SubGroup { get; set; } = string.Empty;

    /// <summary>
    /// Order for display in UI dropdowns
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Whether this category is available for selection
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Optional FontAwesome icon name hint for UI (e.g., "hurricane", "fire")
    /// </summary>
    public string? IconName { get; set; }
}
