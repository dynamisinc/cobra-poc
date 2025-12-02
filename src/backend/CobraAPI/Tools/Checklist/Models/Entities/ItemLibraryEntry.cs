namespace CobraAPI.Tools.Checklist.Models.Entities;

/// <summary>
/// Represents a reusable checklist item in the library.
/// Users can save commonly used items to the library and reuse them when building templates.
/// </summary>
public class ItemLibraryEntry
{
    /// <summary>
    /// Unique identifier for the library item
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The text/description of the checklist item
    /// </summary>
    public string ItemText { get; set; } = string.Empty;

    /// <summary>
    /// Type of item: "checkbox" or "status"
    /// </summary>
    public string ItemType { get; set; } = "checkbox";

    /// <summary>
    /// Category for organization (e.g., "Safety", "Logistics", "Communications")
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// JSON string representing status options for status dropdown items.
    /// Format: [{"label":"Complete","isCompletion":true,"order":1}, ...]
    /// Null for checkbox items.
    /// </summary>
    public string? StatusConfiguration { get; set; }

    /// <summary>
    /// JSON string of ICS positions that can use this item
    /// Format: ["Incident Commander", "Safety Officer"]
    /// Null means available to all positions.
    /// </summary>
    public string? AllowedPositions { get; set; }

    /// <summary>
    /// Default notes that will be pre-filled when this item is used
    /// </summary>
    public string? DefaultNotes { get; set; }

    /// <summary>
    /// JSON array of tags for searchability and filtering
    /// Format: ["safety", "equipment", "daily"]
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Default value for IsRequired when this item is added to a template
    /// </summary>
    public bool IsRequiredByDefault { get; set; } = false;

    /// <summary>
    /// Number of times this item has been used in templates
    /// Useful for showing "popular" items
    /// </summary>
    public int UsageCount { get; set; } = 0;

    /// <summary>
    /// User who created this library item
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// When this library item was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who last modified this library item (null if never modified)
    /// </summary>
    public string? LastModifiedBy { get; set; }

    /// <summary>
    /// When this library item was last modified (null if never modified)
    /// </summary>
    public DateTime? LastModifiedAt { get; set; }

    /// <summary>
    /// Soft delete flag - archived items are hidden but not deleted
    /// </summary>
    public bool IsArchived { get; set; } = false;

    /// <summary>
    /// User who archived this library item (null if not archived)
    /// </summary>
    public string? ArchivedBy { get; set; }

    /// <summary>
    /// When this library item was archived (null if not archived)
    /// </summary>
    public DateTime? ArchivedAt { get; set; }
}
