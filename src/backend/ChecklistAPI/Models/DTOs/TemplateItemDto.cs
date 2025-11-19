namespace ChecklistAPI.Models.DTOs;

/// <summary>
/// TemplateItemDto - Data transfer object for template items
///
/// Purpose:
///   Represents a single item within a checklist template.
///   Used for API responses when returning template data.
///
/// Item Types:
///   - "checkbox": Simple yes/no completion
///   - "status": Dropdown with custom status options
///
/// Display Order:
///   Items are sorted by DisplayOrder when presenting to users.
///   Lower numbers appear first (10, 20, 30...).
///
/// Status Options (for "status" type):
///   Stored as JSON array: ["Not Started", "In Progress", "Completed", "N/A"]
///   Null for "checkbox" type items.
///
/// Author: Checklist POC Team
/// Last Modified: 2025-11-19
/// </summary>
public record TemplateItemDto
{
    /// <summary>
    /// Unique identifier for this template item
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// ID of the parent template
    /// </summary>
    public Guid TemplateId { get; init; }

    /// <summary>
    /// The text/description of the checklist item
    /// Example: "Verify all personnel have safety equipment"
    /// </summary>
    public string ItemText { get; init; } = string.Empty;

    /// <summary>
    /// Type of item: "checkbox" or "status"
    /// </summary>
    public string ItemType { get; init; } = string.Empty;

    /// <summary>
    /// Display order for sorting (10, 20, 30, etc.)
    /// </summary>
    public int DisplayOrder { get; init; }

    /// <summary>
    /// For "status" type items: JSON array of status options
    /// Example: ["Not Started", "In Progress", "Completed", "Blocked"]
    /// Null for "checkbox" type items
    /// </summary>
    public string? StatusOptions { get; init; }

    /// <summary>
    /// Optional notes/instructions for this item
    /// Example: "Check with logistics coordinator before marking complete"
    /// </summary>
    public string? Notes { get; init; }
}
