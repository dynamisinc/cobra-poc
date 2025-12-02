using System.ComponentModel.DataAnnotations;

namespace CobraAPI.Tools.Checklist.Models.DTOs;

/// <summary>
/// UpdateItemStatusRequest - Request DTO for updating status-type items
///
/// Purpose:
///   Updates the current status of status-type checklist items.
///   Used by PATCH /api/checklists/{checklistId}/items/{itemId}/status endpoint.
///
/// Business Rules:
///   - Only applies to status-type items (ItemType = "status")
///   - Status value must be one of the allowed values in StatusOptions
///   - Auto-populates LastModifiedBy, LastModifiedByPosition, LastModifiedAt
///   - Status items are considered "complete" based on status value
///   - Triggers automatic progress recalculation
///
/// Status Completion Logic:
///   Items with status "Complete" or "Completed" are counted as complete
///   for progress tracking purposes.
///
/// User Attribution:
///   LastModifiedBy and LastModifiedByPosition auto-populated from UserContext.
///
/// Author: Checklist POC Team
/// Last Modified: 2025-11-20
/// </summary>
public record UpdateItemStatusRequest
{
    /// <summary>
    /// New status value for the item
    /// Must be one of the values in the item's StatusOptions field
    /// Example: "Not Started", "In Progress", "Complete"
    /// </summary>
    [Required(ErrorMessage = "Status is required")]
    [MaxLength(100, ErrorMessage = "Status cannot exceed 100 characters")]
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Optional notes about the status change
    /// Example: "Waiting on supply delivery"
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; init; }
}
