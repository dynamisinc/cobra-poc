using Microsoft.AspNetCore.Mvc;
using ChecklistAPI.Models;
using ChecklistAPI.Models.DTOs;
using ChecklistAPI.Services;

namespace ChecklistAPI.Controllers;

/// <summary>
/// API controller for managing events (incidents/operations)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _service;
    private readonly ILogger<EventsController> _logger;

    public EventsController(
        IEventService service,
        ILogger<EventsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get all events with optional filtering
    /// </summary>
    /// <param name="eventType">Filter by event type: "PLANNED" or "UNPLANNED" (optional)</param>
    /// <param name="activeOnly">Only return active events (default: true)</param>
    /// <returns>List of events</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EventDto>>> GetEvents(
        [FromQuery] string? eventType = null,
        [FromQuery] bool activeOnly = true)
    {
        _logger.LogInformation(
            "GET /api/events - EventType: {EventType}, ActiveOnly: {ActiveOnly}",
            eventType, activeOnly);

        var events = await _service.GetEventsAsync(eventType, activeOnly);
        return Ok(events);
    }

    /// <summary>
    /// Get a specific event by ID
    /// </summary>
    /// <param name="id">The event ID</param>
    /// <returns>The event if found</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> GetEvent(Guid id)
    {
        _logger.LogInformation("GET /api/events/{Id}", id);

        var eventDto = await _service.GetEventByIdAsync(id);

        if (eventDto == null)
        {
            _logger.LogWarning("Event not found: {Id}", id);
            return NotFound(new { message = $"Event {id} not found" });
        }

        return Ok(eventDto);
    }

    /// <summary>
    /// Create a new event
    /// </summary>
    /// <param name="request">The event data</param>
    /// <returns>The created event</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EventDto>> CreateEvent([FromBody] CreateEventRequest request)
    {
        _logger.LogInformation(
            "POST /api/events - Creating event: {Name}, Type: {EventType}",
            request.Name, request.EventType);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for create event");
            return BadRequest(ModelState);
        }

        try
        {
            var eventDto = await _service.CreateEventAsync(request);
            return CreatedAtAction(
                nameof(GetEvent),
                new { id = eventDto.Id },
                eventDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error creating event");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event");
            throw;
        }
    }

    /// <summary>
    /// Update an existing event
    /// </summary>
    /// <param name="id">The event ID</param>
    /// <param name="request">The updated event data</param>
    /// <returns>The updated event</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> UpdateEvent(Guid id, [FromBody] UpdateEventRequest request)
    {
        _logger.LogInformation("PUT /api/events/{Id}", id);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for update event");
            return BadRequest(ModelState);
        }

        try
        {
            var eventDto = await _service.UpdateEventAsync(id, request);
            return Ok(eventDto);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Event {id} not found" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Archive an event (soft delete)
    /// </summary>
    /// <param name="id">The event ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ArchiveEvent(Guid id)
    {
        _logger.LogInformation("DELETE /api/events/{Id} - Archiving", id);

        try
        {
            await _service.ArchiveEventAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Event {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving event {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Restore an archived event
    /// </summary>
    /// <param name="id">The event ID</param>
    /// <returns>No content on success</returns>
    [HttpPost("{id}/restore")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreEvent(Guid id)
    {
        _logger.LogInformation("POST /api/events/{Id}/restore", id);

        try
        {
            await _service.RestoreEventAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Archived event {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring event {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Permanently delete an event (Manage role)
    /// Will fail if event has associated checklists.
    /// </summary>
    /// <param name="id">The event ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}/permanent")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEventPermanently(Guid id)
    {
        var userContext = GetUserContext();

        if (!userContext.CanManage)
        {
            _logger.LogWarning(
                "User {User} with role {Role} attempted to permanently delete event {EventId}",
                userContext.Email,
                userContext.Role,
                id);
            return Forbid();
        }

        _logger.LogInformation("DELETE /api/events/{Id}/permanent - Permanently deleting", id);

        try
        {
            await _service.DeleteEventAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Event {id} not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error permanently deleting event {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Set an event's active status
    /// </summary>
    /// <param name="id">The event ID</param>
    /// <param name="isActive">Whether the event should be active</param>
    /// <returns>No content on success</returns>
    [HttpPatch("{id}/active")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetEventActive(Guid id, [FromQuery] bool isActive)
    {
        _logger.LogInformation("PATCH /api/events/{Id}/active?isActive={IsActive}", id, isActive);

        try
        {
            await _service.SetEventActiveAsync(id, isActive);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Event {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting event {Id} active status", id);
            throw;
        }
    }

    /// <summary>
    /// Extract user context from HttpContext (set by MockUserMiddleware)
    /// </summary>
    private UserContext GetUserContext()
    {
        return HttpContext.Items["UserContext"] as UserContext
            ?? throw new InvalidOperationException("User context not found - is MockUserMiddleware configured?");
    }
}
