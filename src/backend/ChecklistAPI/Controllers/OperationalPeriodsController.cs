using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChecklistAPI.Data;
using ChecklistAPI.Models.Entities;

namespace ChecklistAPI.Controllers;

/// <summary>
/// Operational Periods Controller - FOR POC TESTING ONLY
///
/// In production, C5 application will manage operational periods.
/// These endpoints are for testing period grouping in frontend.
/// </summary>
[ApiController]
[Route("api/operational-periods")]
[Produces("application/json")]
public class OperationalPeriodsController : ControllerBase
{
    private readonly ChecklistDbContext _context;
    private readonly ILogger<OperationalPeriodsController> _logger;

    public OperationalPeriodsController(
        ChecklistDbContext context,
        ILogger<OperationalPeriodsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all operational periods for an event
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<OperationalPeriod>>> GetOperationalPeriods(
        [FromQuery] Guid? eventId,
        [FromQuery] bool includeArchived = false)
    {
        _logger.LogInformation("Fetching operational periods for event: {EventId}, includeArchived: {IncludeArchived}",
            eventId, includeArchived);

        var query = _context.OperationalPeriods.AsQueryable();

        // Filter by event
        if (eventId.HasValue)
        {
            query = query.Where(op => op.EventId == eventId.Value);
        }

        // Filter archived
        if (!includeArchived)
        {
            query = query.Where(op => !op.IsArchived);
        }

        var periods = await query
            .OrderBy(op => op.StartTime)
            .ToListAsync();

        _logger.LogInformation("Returning {Count} operational periods", periods.Count);
        return Ok(periods);
    }

    /// <summary>
    /// Get a specific operational period by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OperationalPeriod>> GetOperationalPeriod(Guid id)
    {
        _logger.LogInformation("Fetching operational period: {PeriodId}", id);

        var period = await _context.OperationalPeriods.FindAsync(id);

        if (period == null)
        {
            _logger.LogWarning("Operational period {PeriodId} not found", id);
            return NotFound(new { message = $"Operational period {id} not found" });
        }

        return Ok(period);
    }

    /// <summary>
    /// Get the current operational period for an event
    /// </summary>
    [HttpGet("current")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OperationalPeriod>> GetCurrentPeriod([FromQuery] Guid eventId)
    {
        _logger.LogInformation("Fetching current operational period for event: {EventId}", eventId);

        var currentPeriod = await _context.OperationalPeriods
            .Where(op => op.EventId == eventId && op.IsCurrent && !op.IsArchived)
            .FirstOrDefaultAsync();

        if (currentPeriod == null)
        {
            _logger.LogInformation("No current operational period for event {EventId}", eventId);
            return NotFound(new { message = $"No current operational period for event {eventId}" });
        }

        return Ok(currentPeriod);
    }

    /// <summary>
    /// Create a new operational period (TEST ONLY)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OperationalPeriod>> CreatePeriod([FromBody] CreateOperationalPeriodRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _logger.LogInformation("Creating operational period: {Name} for event {EventId}",
            request.Name, request.EventId);

        var period = new OperationalPeriod
        {
            Id = Guid.NewGuid(),
            EventId = request.EventId,
            Name = request.Name,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            IsCurrent = request.IsCurrent,
            Description = request.Description,
            CreatedBy = HttpContext.Request.Headers["X-User-FullName"].ToString() ?? "Test User",
            CreatedAt = DateTime.UtcNow
        };

        // If setting as current, unset other current periods for this event
        if (period.IsCurrent)
        {
            var existingCurrent = await _context.OperationalPeriods
                .Where(op => op.EventId == period.EventId && op.IsCurrent)
                .ToListAsync();

            foreach (var existing in existingCurrent)
            {
                existing.IsCurrent = false;
                existing.LastModifiedBy = period.CreatedBy;
                existing.LastModifiedAt = DateTime.UtcNow;
            }

            _logger.LogInformation("Unset {Count} existing current periods", existingCurrent.Count);
        }

        _context.OperationalPeriods.Add(period);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created operational period {PeriodId}: {Name}", period.Id, period.Name);

        return CreatedAtAction(
            nameof(GetOperationalPeriod),
            new { id = period.Id },
            period);
    }

    /// <summary>
    /// Set a period as the current operational period (TEST ONLY)
    /// Unsets all other current periods for the same event
    /// </summary>
    [HttpPost("{id}/set-current")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OperationalPeriod>> SetCurrentPeriod(Guid id)
    {
        _logger.LogInformation("Setting operational period {PeriodId} as current", id);

        var period = await _context.OperationalPeriods.FindAsync(id);
        if (period == null)
        {
            _logger.LogWarning("Operational period {PeriodId} not found", id);
            return NotFound(new { message = $"Operational period {id} not found" });
        }

        // Unset all other current periods for this event
        var otherCurrentPeriods = await _context.OperationalPeriods
            .Where(op => op.EventId == period.EventId && op.IsCurrent && op.Id != id)
            .ToListAsync();

        foreach (var other in otherCurrentPeriods)
        {
            other.IsCurrent = false;
            other.LastModifiedBy = HttpContext.Request.Headers["X-User-FullName"].ToString() ?? "Test User";
            other.LastModifiedAt = DateTime.UtcNow;
        }

        // Set this period as current
        period.IsCurrent = true;
        period.LastModifiedBy = HttpContext.Request.Headers["X-User-FullName"].ToString() ?? "Test User";
        period.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Set operational period {PeriodId} as current. Unset {Count} other periods.",
            id, otherCurrentPeriods.Count);

        return Ok(period);
    }

    /// <summary>
    /// Soft delete (archive) an operational period (TEST ONLY)
    /// Checklists with this period will have their OperationalPeriodId set to NULL
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ArchivePeriod(Guid id)
    {
        _logger.LogInformation("Archiving operational period: {PeriodId}", id);

        var period = await _context.OperationalPeriods.FindAsync(id);
        if (period == null)
        {
            _logger.LogWarning("Operational period {PeriodId} not found", id);
            return NotFound(new { message = $"Operational period {id} not found" });
        }

        period.IsArchived = true;
        period.ArchivedBy = HttpContext.Request.Headers["X-User-FullName"].ToString() ?? "Test User";
        period.ArchivedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Archived operational period {PeriodId}. Associated checklists will show as incident-level.",
            id);

        return Ok(new { message = $"Operational period '{period.Name}' archived successfully" });
    }
}

/// <summary>
/// Request model for creating operational periods
/// </summary>
public record CreateOperationalPeriodRequest(
    Guid EventId,
    string Name,
    DateTime StartTime,
    DateTime? EndTime,
    bool IsCurrent,
    string? Description
);
