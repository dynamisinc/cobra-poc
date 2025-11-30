using Microsoft.EntityFrameworkCore;
using ChecklistAPI.Data;
using ChecklistAPI.Models.DTOs;

namespace ChecklistAPI.Services;

/// <summary>
/// Service for managing event categories (FEMA/NIMS standard categories)
/// Categories are seeded on database creation and are read-only for POC.
/// </summary>
public class EventCategoryService : IEventCategoryService
{
    private readonly ChecklistDbContext _context;
    private readonly ILogger<EventCategoryService> _logger;

    public EventCategoryService(
        ChecklistDbContext context,
        ILogger<EventCategoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<EventCategoryDto>> GetCategoriesAsync(string? eventType = null)
    {
        _logger.LogInformation("Getting event categories with filter - EventType: {EventType}", eventType);

        var query = _context.EventCategories
            .Where(c => c.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(eventType))
        {
            query = query.Where(c => c.EventType.ToLower() == eventType.ToLower());
        }

        var categories = await query
            .OrderBy(c => c.EventType)
            .ThenBy(c => c.SubGroup)
            .ThenBy(c => c.DisplayOrder)
            .ToListAsync();

        _logger.LogInformation("Found {Count} event categories", categories.Count);

        return categories.Select(MapToDto).ToList();
    }

    public async Task<EventCategoryDto?> GetCategoryByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting event category by ID: {Id}", id);

        var category = await _context.EventCategories
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            _logger.LogWarning("Event category not found: {Id}", id);
            return null;
        }

        return MapToDto(category);
    }

    public async Task<EventCategoryDto?> GetCategoryByCodeAsync(string code)
    {
        _logger.LogInformation("Getting event category by code: {Code}", code);

        var category = await _context.EventCategories
            .FirstOrDefaultAsync(c => c.Code == code.ToUpper());

        if (category == null)
        {
            _logger.LogWarning("Event category not found by code: {Code}", code);
            return null;
        }

        return MapToDto(category);
    }

    public async Task<Dictionary<string, List<EventCategoryDto>>> GetCategoriesGroupedAsync(string eventType)
    {
        _logger.LogInformation("Getting event categories grouped by SubGroup for EventType: {EventType}", eventType);

        var categories = await _context.EventCategories
            .Where(c => c.IsActive && c.EventType.ToLower() == eventType.ToLower())
            .OrderBy(c => c.SubGroup)
            .ThenBy(c => c.DisplayOrder)
            .ToListAsync();

        var grouped = categories
            .GroupBy(c => c.SubGroup)
            .ToDictionary(
                g => g.Key,
                g => g.Select(MapToDto).ToList()
            );

        _logger.LogInformation("Found {GroupCount} category groups with {TotalCount} categories",
            grouped.Count, categories.Count);

        return grouped;
    }

    private static EventCategoryDto MapToDto(Models.Entities.EventCategory category)
    {
        return new EventCategoryDto
        {
            Id = category.Id,
            Code = category.Code,
            Name = category.Name,
            EventType = category.EventType,
            SubGroup = category.SubGroup,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            IconName = category.IconName
        };
    }
}
