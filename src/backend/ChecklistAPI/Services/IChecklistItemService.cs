using ChecklistAPI.Models;
using ChecklistAPI.Models.DTOs;

namespace ChecklistAPI.Services;

/// <summary>
/// IChecklistItemService - Interface for checklist item operations
///
/// Purpose:
///   Defines the contract for checklist item completion and status operations.
///   Controllers call service methods, services orchestrate business logic.
///
/// Single Responsibility:
///   This service ONLY handles individual ChecklistItem operations.
///   Checklist-level operations are in IChecklistService.
///
/// Key Business Logic:
///   - Item Completion: Mark checkbox items complete/incomplete
///   - Status Updates: Update status-type items
///   - Notes Management: Add/update item notes
///   - Progress Tracking: Auto-trigger checklist progress recalculation
///   - Position Permissions: Validate AllowedPositions before updates
///
/// Dependency Injection:
///   Registered as scoped service in Program.cs:
///   builder.Services.AddScoped<IChecklistItemService, ChecklistItemService>();
///
/// Design Pattern:
///   - Interface for testability and loose coupling
///   - Services receive UserContext for audit trail
///   - Services throw exceptions, controllers handle HTTP responses
///   - All async for database operations
///
/// Author: Checklist POC Team
/// Last Modified: 2025-11-20
/// </summary>
public interface IChecklistItemService
{
    /// <summary>
    /// Get a single checklist item by ID
    /// Used to retrieve item details before updates
    /// </summary>
    /// <param name="checklistId">Checklist GUID</param>
    /// <param name="itemId">Item GUID</param>
    /// <returns>Item details, or null if not found</returns>
    Task<ChecklistItemDto?> GetItemByIdAsync(Guid checklistId, Guid itemId);

    /// <summary>
    /// Update completion status of a checkbox-type item
    /// Automatically populates CompletedBy, CompletedByPosition, CompletedAt
    /// Triggers progress recalculation for the parent checklist
    /// </summary>
    /// <param name="checklistId">Checklist GUID</param>
    /// <param name="itemId">Item GUID</param>
    /// <param name="request">Completion data (IsCompleted, optional Notes)</param>
    /// <param name="userContext">Current user context for audit trail</param>
    /// <returns>Updated item, or null if not found</returns>
    /// <exception cref="InvalidOperationException">If item is not checkbox type</exception>
    /// <exception cref="UnauthorizedAccessException">If user position not allowed</exception>
    Task<ChecklistItemDto?> UpdateItemCompletionAsync(
        Guid checklistId,
        Guid itemId,
        UpdateItemCompletionRequest request,
        UserContext userContext);

    /// <summary>
    /// Update status of a status-type item
    /// Validates status value is in allowed StatusOptions
    /// Triggers progress recalculation (status items count as complete if status = "Complete")
    /// </summary>
    /// <param name="checklistId">Checklist GUID</param>
    /// <param name="itemId">Item GUID</param>
    /// <param name="request">Status data (Status value, optional Notes)</param>
    /// <param name="userContext">Current user context for audit trail</param>
    /// <returns>Updated item, or null if not found</returns>
    /// <exception cref="InvalidOperationException">If item is not status type or status invalid</exception>
    /// <exception cref="UnauthorizedAccessException">If user position not allowed</exception>
    Task<ChecklistItemDto?> UpdateItemStatusAsync(
        Guid checklistId,
        Guid itemId,
        UpdateItemStatusRequest request,
        UserContext userContext);

    /// <summary>
    /// Update notes on any checklist item
    /// Works for both checkbox and status items
    /// Does NOT trigger progress recalculation
    /// </summary>
    /// <param name="checklistId">Checklist GUID</param>
    /// <param name="itemId">Item GUID</param>
    /// <param name="request">Notes data</param>
    /// <param name="userContext">Current user context for audit trail</param>
    /// <returns>Updated item, or null if not found</returns>
    /// <exception cref="UnauthorizedAccessException">If user position not allowed</exception>
    Task<ChecklistItemDto?> UpdateItemNotesAsync(
        Guid checklistId,
        Guid itemId,
        UpdateItemNotesRequest request,
        UserContext userContext);
}
