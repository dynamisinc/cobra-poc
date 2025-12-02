
namespace CobraAPI.Tools.Checklist.Services;

/// <summary>
/// Service interface for managing item library entries
/// </summary>
public interface IItemLibraryService
{
    /// <summary>
    /// Get all non-archived library items with optional filtering
    /// </summary>
    /// <param name="category">Filter by category (optional)</param>
    /// <param name="itemType">Filter by item type (optional)</param>
    /// <param name="searchText">Search in item text and tags (optional)</param>
    /// <param name="sortBy">Sort order: "recent", "popular", "alphabetical" (default: "recent")</param>
    Task<List<ItemLibraryEntryDto>> GetLibraryItemsAsync(
        string? category = null,
        string? itemType = null,
        string? searchText = null,
        string sortBy = "recent");

    /// <summary>
    /// Get a specific library item by ID
    /// </summary>
    Task<ItemLibraryEntryDto?> GetLibraryItemByIdAsync(Guid id);

    /// <summary>
    /// Create a new library item
    /// </summary>
    Task<ItemLibraryEntryDto> CreateLibraryItemAsync(CreateItemLibraryEntryRequest request);

    /// <summary>
    /// Update an existing library item
    /// </summary>
    Task<ItemLibraryEntryDto> UpdateLibraryItemAsync(Guid id, UpdateItemLibraryEntryRequest request);

    /// <summary>
    /// Increment the usage count for a library item (called when added to a template)
    /// </summary>
    Task IncrementUsageCountAsync(Guid id);

    /// <summary>
    /// Archive a library item (soft delete)
    /// </summary>
    Task ArchiveLibraryItemAsync(Guid id);

    /// <summary>
    /// Restore an archived library item
    /// </summary>
    Task RestoreLibraryItemAsync(Guid id);

    /// <summary>
    /// Permanently delete a library item (admin only)
    /// </summary>
    Task DeleteLibraryItemAsync(Guid id);
}
