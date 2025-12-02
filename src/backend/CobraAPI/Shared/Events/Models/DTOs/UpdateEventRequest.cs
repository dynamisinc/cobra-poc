using System.ComponentModel.DataAnnotations;

namespace CobraAPI.Shared.Events.Models.DTOs;

/// <summary>
/// UpdateEventRequest - Request DTO for updating an existing event
///
/// Note: EventType cannot be changed after creation as it would
/// invalidate the category relationships.
///
/// Author: Checklist POC Team
/// Last Modified: 2025-11-29
/// </summary>
public record UpdateEventRequest
{
    /// <summary>
    /// Updated event name
    /// </summary>
    [Required(ErrorMessage = "Event name is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Name must be 3-200 characters")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Updated primary category ID
    /// Must match the event's EventType
    /// </summary>
    [Required(ErrorMessage = "Primary category is required")]
    public Guid PrimaryCategoryId { get; init; }

    /// <summary>
    /// Updated additional category IDs
    /// </summary>
    public List<Guid>? AdditionalCategoryIds { get; init; }

    /// <summary>
    /// Whether the event is active
    /// </summary>
    public bool IsActive { get; init; } = true;
}
