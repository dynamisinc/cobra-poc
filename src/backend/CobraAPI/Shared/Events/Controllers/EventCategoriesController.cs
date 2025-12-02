using Microsoft.AspNetCore.Mvc;

namespace CobraAPI.Shared.Events.Controllers;

/// <summary>
/// API controller for event categories (FEMA/NIMS standard categories)
/// Categories are read-only - they are seeded on database creation.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventCategoriesController : ControllerBase
{
    private readonly IEventCategoryService _service;
    private readonly ILogger<EventCategoriesController> _logger;

    public EventCategoriesController(
        IEventCategoryService service,
        ILogger<EventCategoriesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get all event categories with optional filtering by event type
    /// </summary>
    /// <param name="eventType">Filter by event type: "PLANNED" or "UNPLANNED" (optional)</param>
    /// <returns>List of event categories</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EventCategoryDto>>> GetCategories(
        [FromQuery] string? eventType = null)
    {
        _logger.LogInformation("GET /api/eventcategories - EventType: {EventType}", eventType);

        var categories = await _service.GetCategoriesAsync(eventType);
        return Ok(categories);
    }

    /// <summary>
    /// Get event categories grouped by SubGroup for UI display
    /// </summary>
    /// <param name="eventType">Event type: "PLANNED" or "UNPLANNED" (required)</param>
    /// <returns>Categories grouped by SubGroup</returns>
    [HttpGet("grouped")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Dictionary<string, List<EventCategoryDto>>>> GetCategoriesGrouped(
        [FromQuery] string eventType)
    {
        _logger.LogInformation("GET /api/eventcategories/grouped - EventType: {EventType}", eventType);

        if (string.IsNullOrWhiteSpace(eventType) ||
            (eventType.ToUpper() != "PLANNED" && eventType.ToUpper() != "UNPLANNED"))
        {
            return BadRequest(new { message = "eventType query parameter is required and must be PLANNED or UNPLANNED" });
        }

        var grouped = await _service.GetCategoriesGroupedAsync(eventType);
        return Ok(grouped);
    }

    /// <summary>
    /// Get a specific event category by ID
    /// </summary>
    /// <param name="id">The category ID</param>
    /// <returns>The category if found</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventCategoryDto>> GetCategory(Guid id)
    {
        _logger.LogInformation("GET /api/eventcategories/{Id}", id);

        var category = await _service.GetCategoryByIdAsync(id);

        if (category == null)
        {
            _logger.LogWarning("Event category not found: {Id}", id);
            return NotFound(new { message = $"Event category {id} not found" });
        }

        return Ok(category);
    }

    /// <summary>
    /// Get a specific event category by code
    /// </summary>
    /// <param name="code">The category code (e.g., "HURRICANE", "PARADE")</param>
    /// <returns>The category if found</returns>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventCategoryDto>> GetCategoryByCode(string code)
    {
        _logger.LogInformation("GET /api/eventcategories/by-code/{Code}", code);

        var category = await _service.GetCategoryByCodeAsync(code);

        if (category == null)
        {
            _logger.LogWarning("Event category not found by code: {Code}", code);
            return NotFound(new { message = $"Event category with code '{code}' not found" });
        }

        return Ok(category);
    }
}
