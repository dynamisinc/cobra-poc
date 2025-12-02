using System.ComponentModel.DataAnnotations;

namespace CobraAPI.Tools.Checklist.Models.DTOs;

/// <summary>
/// CreateTemplateItemRequest - Request DTO for creating template items
///
/// Purpose:
///   Captures the data needed to create a new item within a template.
///   Used when creating or updating templates with their items.
///
/// Validation Rules:
///   - ItemText: Required, max 500 characters
///   - ItemType: Required, must be "checkbox" or "status"
///   - DisplayOrder: Required, positive integer
///   - StatusOptions: Required if ItemType is "status", null otherwise
///
/// Author: Checklist POC Team
/// Last Modified: 2025-11-19
/// </summary>
public record CreateTemplateItemRequest
{
    /// <summary>
    /// The text/description of the checklist item
    /// Example: "Verify all personnel have safety equipment"
    /// </summary>
    [Required(ErrorMessage = "Item text is required")]
    [MaxLength(500, ErrorMessage = "Item text cannot exceed 500 characters")]
    public string ItemText { get; init; } = string.Empty;

    /// <summary>
    /// Type of item: "checkbox" or "status"
    /// </summary>
    [Required(ErrorMessage = "Item type is required")]
    [RegularExpression("^(checkbox|status)$", ErrorMessage = "Item type must be 'checkbox' or 'status'")]
    public string ItemType { get; init; } = string.Empty;

    /// <summary>
    /// Display order for sorting (10, 20, 30, etc.)
    /// Lower numbers appear first
    /// </summary>
    [Required(ErrorMessage = "Display order is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Display order must be positive")]
    public int DisplayOrder { get; init; }

    /// <summary>
    /// For "status" type items: JSON array of status configuration objects
    /// Example: [{"label":"Not Started","isCompletion":false,"order":1}, ...]
    /// Must be null for "checkbox" type items
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Status configuration cannot exceed 1000 characters")]
    public string? StatusConfiguration { get; init; }

    /// <summary>
    /// Optional notes/instructions for this item
    /// Example: "Check with logistics coordinator before marking complete"
    /// </summary>
    [MaxLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; init; }
}
