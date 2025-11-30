using ChecklistAPI.Models;
using ChecklistAPI.Models.DTOs;
using ChecklistAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChecklistAPI.Controllers;

/// <summary>
/// ChecklistItemsController - API endpoints for checklist item operations
///
/// Purpose:
///   Provides RESTful endpoints for individual checklist item operations.
///   Enables users to mark items complete, update status, and add notes.
///   Thin controller pattern: validation and routing only, business logic in service.
///
/// Base Route: /api/checklists/{checklistId}/items
///
/// Endpoints:
///   GET   /api/checklists/{checklistId}/items/{itemId}            - Get single item
///   PATCH /api/checklists/{checklistId}/items/{itemId}/completion - Update completion status
///   PATCH /api/checklists/{checklistId}/items/{itemId}/status     - Update status value
///   PATCH /api/checklists/{checklistId}/items/{itemId}/notes      - Update notes
///
/// User Context:
///   Automatically injected by MockUserMiddleware (POC)
///   Controllers extract from HttpContext.Items["UserContext"]
///
/// Position-Based Permissions:
///   All update operations check AllowedPositions field.
///   Service throws UnauthorizedAccessException if user position not allowed.
///
/// Progress Tracking:
///   Completion and status updates automatically trigger progress recalculation.
///   Notes-only updates do NOT trigger progress recalculation.
///
/// Error Handling:
///   - 400 Bad Request: Validation failures
///   - 403 Forbidden: Position not authorized, or wrong item type
///   - 404 Not Found: Item doesn't exist
///   - 500 Internal Server Error: Unhandled exceptions (logged to App Insights)
///
/// Author: Checklist POC Team
/// Last Modified: 2025-11-20
/// </summary>
[ApiController]
[Route("api/checklists/{checklistId:guid}/items")]
public class ChecklistItemsController : ControllerBase
{
    private readonly IChecklistItemService _itemService;
    private readonly ILogger<ChecklistItemsController> _logger;

    public ChecklistItemsController(
        IChecklistItemService itemService,
        ILogger<ChecklistItemsController> logger)
    {
        _itemService = itemService;
        _logger = logger;
    }

    /// <summary>
    /// Get a single checklist item by ID
    /// </summary>
    /// <param name="checklistId">Checklist GUID</param>
    /// <param name="itemId">Item GUID</param>
    /// <returns>Item details</returns>
    [HttpGet("{itemId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChecklistItemDto>> GetItem(Guid checklistId, Guid itemId)
    {
        var item = await _itemService.GetItemByIdAsync(checklistId, itemId);

        if (item == null)
        {
            _logger.LogWarning(
                "Item {ItemId} not found in checklist {ChecklistId}",
                itemId,
                checklistId);
            return NotFound(new
            {
                message = $"Item {itemId} not found in checklist {checklistId}"
            });
        }

        return Ok(item);
    }

    /// <summary>
    /// Update completion status of a checkbox-type item
    /// Automatically populates CompletedBy, CompletedByPosition, CompletedAt
    /// Triggers progress recalculation for the parent checklist
    /// </summary>
    /// <param name="checklistId">Checklist GUID</param>
    /// <param name="itemId">Item GUID</param>
    /// <param name="request">Completion data (IsCompleted, optional Notes)</param>
    /// <returns>Updated item</returns>
    [HttpPatch("{itemId:guid}/completion")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChecklistItemDto>> UpdateItemCompletion(
        Guid checklistId,
        Guid itemId,
        [FromBody] UpdateItemCompletionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userContext = GetUserContext();

        // Readonly users cannot modify items
        if (userContext.IsReadonly)
        {
            _logger.LogWarning(
                "Readonly user {User} attempted to update item completion",
                userContext.Email);
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "Readonly users cannot modify checklist items"
            });
        }

        try
        {
            var item = await _itemService.UpdateItemCompletionAsync(
                checklistId,
                itemId,
                request,
                userContext);

            if (item == null)
            {
                _logger.LogWarning(
                    "Item {ItemId} not found in checklist {ChecklistId}",
                    itemId,
                    checklistId);
                return NotFound(new
                {
                    message = $"Item {itemId} not found in checklist {checklistId}"
                });
            }

            _logger.LogInformation(
                "Item {ItemId} completion updated to {IsCompleted} by {User}",
                itemId,
                request.IsCompleted,
                userContext.Email);

            return Ok(item);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation for item {ItemId}", itemId);
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt for item {ItemId}", itemId);
            return Forbid();
        }
    }

    /// <summary>
    /// Update status of a status-type item
    /// Validates status value is in allowed StatusOptions
    /// Triggers progress recalculation (status items count as complete if status = "Complete")
    /// </summary>
    /// <param name="checklistId">Checklist GUID</param>
    /// <param name="itemId">Item GUID</param>
    /// <param name="request">Status data (Status value, optional Notes)</param>
    /// <returns>Updated item</returns>
    [HttpPatch("{itemId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChecklistItemDto>> UpdateItemStatus(
        Guid checklistId,
        Guid itemId,
        [FromBody] UpdateItemStatusRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userContext = GetUserContext();

        // Readonly users cannot modify items
        if (userContext.IsReadonly)
        {
            _logger.LogWarning(
                "Readonly user {User} attempted to update item status",
                userContext.Email);
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "Readonly users cannot modify checklist items"
            });
        }

        try
        {
            var item = await _itemService.UpdateItemStatusAsync(
                checklistId,
                itemId,
                request,
                userContext);

            if (item == null)
            {
                _logger.LogWarning(
                    "Item {ItemId} not found in checklist {ChecklistId}",
                    itemId,
                    checklistId);
                return NotFound(new
                {
                    message = $"Item {itemId} not found in checklist {checklistId}"
                });
            }

            _logger.LogInformation(
                "Item {ItemId} status updated to '{Status}' by {User}",
                itemId,
                request.Status,
                userContext.Email);

            return Ok(item);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation for item {ItemId}", itemId);
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt for item {ItemId}", itemId);
            return Forbid();
        }
    }

    /// <summary>
    /// Update notes on any checklist item
    /// Works for both checkbox and status items
    /// Does NOT trigger progress recalculation
    /// </summary>
    /// <param name="checklistId">Checklist GUID</param>
    /// <param name="itemId">Item GUID</param>
    /// <param name="request">Notes data</param>
    /// <returns>Updated item</returns>
    [HttpPatch("{itemId:guid}/notes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChecklistItemDto>> UpdateItemNotes(
        Guid checklistId,
        Guid itemId,
        [FromBody] UpdateItemNotesRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userContext = GetUserContext();

        // Readonly users cannot modify items
        if (userContext.IsReadonly)
        {
            _logger.LogWarning(
                "Readonly user {User} attempted to update item notes",
                userContext.Email);
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "Readonly users cannot modify checklist items"
            });
        }

        try
        {
            var item = await _itemService.UpdateItemNotesAsync(
                checklistId,
                itemId,
                request,
                userContext);

            if (item == null)
            {
                _logger.LogWarning(
                    "Item {ItemId} not found in checklist {ChecklistId}",
                    itemId,
                    checklistId);
                return NotFound(new
                {
                    message = $"Item {itemId} not found in checklist {checklistId}"
                });
            }

            _logger.LogInformation(
                "Item {ItemId} notes updated by {User}",
                itemId,
                userContext.Email);

            return Ok(item);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt for item {ItemId}", itemId);
            return Forbid();
        }
    }

    /// <summary>
    /// Extract UserContext from HttpContext (injected by middleware)
    /// Falls back to default if not found (should never happen in POC)
    /// </summary>
    private UserContext GetUserContext()
    {
        if (HttpContext.Items.TryGetValue("UserContext", out var context) &&
            context is UserContext userContext)
        {
            return userContext;
        }

        _logger.LogWarning("UserContext not found in HttpContext, using default");
        return new UserContext
        {
            Email = "unknown@cobra.mil",
            FullName = "Unknown User",
            Position = "Unknown",
            IsAdmin = false
        };
    }
}
