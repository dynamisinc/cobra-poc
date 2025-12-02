using System.ComponentModel.DataAnnotations;

namespace CobraAPI.Tools.Checklist.Models.DTOs;

/// <summary>
/// UpdateItemNotesRequest - Request DTO for adding/updating item notes
///
/// Purpose:
///   Adds or updates notes/comments on any checklist item.
///   Used by PATCH /api/checklists/{checklistId}/items/{itemId}/notes endpoint.
///
/// Business Rules:
///   - Works for both checkbox and status-type items
///   - Replaces existing notes (not append)
///   - Auto-populates LastModifiedBy, LastModifiedByPosition, LastModifiedAt
///   - Does NOT trigger progress recalculation (notes don't affect completion)
///
/// Use Cases:
///   - Adding context: "Item completed at 14:30, supplies restocked"
///   - Recording issues: "Minor damage observed, will monitor"
///   - Team communication: "Please verify before signing off"
///
/// User Attribution:
///   LastModifiedBy and LastModifiedByPosition auto-populated from UserContext.
///
/// Author: Checklist POC Team
/// Last Modified: 2025-11-20
/// </summary>
public record UpdateItemNotesRequest
{
    /// <summary>
    /// Notes/comments for the item
    /// Can be null or empty to clear existing notes
    /// </summary>
    [MaxLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; init; }
}
