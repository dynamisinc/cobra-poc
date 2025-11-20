using System.ComponentModel.DataAnnotations;

namespace ChecklistAPI.Models.DTOs;

/// <summary>
/// UpdateItemCompletionRequest - Request DTO for marking items complete/incomplete
///
/// Purpose:
///   Updates the completion status of checkbox-type checklist items.
///   Used by PATCH /api/checklists/{checklistId}/items/{itemId}/complete endpoint.
///
/// Business Rules:
///   - Only applies to checkbox-type items (ItemType = "checkbox")
///   - Sets IsCompleted to true/false
///   - Auto-populates CompletedBy, CompletedByPosition, CompletedAt from UserContext
///   - Triggers automatic progress recalculation for the checklist
///
/// User Attribution:
///   CompletedBy and CompletedByPosition are NOT in this request.
///   They are automatically populated from UserContext by the service layer.
///
/// Author: Checklist POC Team
/// Last Modified: 2025-11-20
/// </summary>
public record UpdateItemCompletionRequest
{
    /// <summary>
    /// Whether the item is completed
    /// true = mark as complete, false = mark as incomplete
    /// </summary>
    [Required(ErrorMessage = "IsCompleted is required")]
    public bool IsCompleted { get; init; }

    /// <summary>
    /// Optional notes about the completion
    /// Example: "Verified all PPE is properly stored"
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; init; }
}
