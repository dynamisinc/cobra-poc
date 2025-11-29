using ChecklistAPI.Models.DTOs;

namespace ChecklistAPI.Services;

/// <summary>
/// Service interface for managing event categories
/// Categories are based on FEMA/NIMS standards and are seeded on database creation.
/// </summary>
public interface IEventCategoryService
{
    /// <summary>
    /// Get all active event categories
    /// </summary>
    /// <param name="eventType">Filter by event type: "PLANNED" or "UNPLANNED" (optional)</param>
    Task<List<EventCategoryDto>> GetCategoriesAsync(string? eventType = null);

    /// <summary>
    /// Get a specific category by ID
    /// </summary>
    Task<EventCategoryDto?> GetCategoryByIdAsync(Guid id);

    /// <summary>
    /// Get a specific category by code
    /// </summary>
    Task<EventCategoryDto?> GetCategoryByCodeAsync(string code);

    /// <summary>
    /// Get categories grouped by SubGroup for UI display
    /// </summary>
    Task<Dictionary<string, List<EventCategoryDto>>> GetCategoriesGroupedAsync(string eventType);
}
