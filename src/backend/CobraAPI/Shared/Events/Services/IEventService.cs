
namespace CobraAPI.Shared.Events.Services;

/// <summary>
/// Service interface for managing events (incidents/operations)
/// </summary>
public interface IEventService
{
    /// <summary>
    /// Get all non-archived events
    /// </summary>
    /// <param name="eventType">Filter by event type: "PLANNED" or "UNPLANNED" (optional)</param>
    /// <param name="activeOnly">Only return active events (default: true)</param>
    Task<List<EventDto>> GetEventsAsync(string? eventType = null, bool activeOnly = true);

    /// <summary>
    /// Get a specific event by ID
    /// </summary>
    Task<EventDto?> GetEventByIdAsync(Guid id);

    /// <summary>
    /// Create a new event
    /// </summary>
    Task<EventDto> CreateEventAsync(CreateEventRequest request);

    /// <summary>
    /// Update an existing event
    /// </summary>
    Task<EventDto> UpdateEventAsync(Guid id, UpdateEventRequest request);

    /// <summary>
    /// Archive an event (soft delete)
    /// </summary>
    Task ArchiveEventAsync(Guid id);

    /// <summary>
    /// Restore an archived event
    /// </summary>
    Task RestoreEventAsync(Guid id);

    /// <summary>
    /// Permanently delete an event (admin only)
    /// Will fail if event has associated checklists
    /// </summary>
    Task DeleteEventAsync(Guid id);

    /// <summary>
    /// Set an event as active/inactive
    /// </summary>
    Task SetEventActiveAsync(Guid id, bool isActive);
}
