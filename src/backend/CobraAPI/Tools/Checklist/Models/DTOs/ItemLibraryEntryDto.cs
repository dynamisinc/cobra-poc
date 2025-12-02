namespace CobraAPI.Tools.Checklist.Models.DTOs;

/// <summary>
/// Data transfer object for item library entries
/// </summary>
public record ItemLibraryEntryDto(
    Guid Id,
    string ItemText,
    string ItemType,
    string Category,
    string? StatusConfiguration,
    string? AllowedPositions,
    string? DefaultNotes,
    string? Tags,
    bool IsRequiredByDefault,
    int UsageCount,
    string CreatedBy,
    DateTime CreatedAt,
    string? LastModifiedBy,
    DateTime? LastModifiedAt
);

/// <summary>
/// Request to create a new item library entry
/// </summary>
public record CreateItemLibraryEntryRequest(
    string ItemText,
    string ItemType,
    string Category,
    string? StatusConfiguration,
    string? AllowedPositions,
    string? DefaultNotes,
    string[]? Tags,
    bool IsRequiredByDefault
);

/// <summary>
/// Request to update an existing item library entry
/// </summary>
public record UpdateItemLibraryEntryRequest(
    string ItemText,
    string ItemType,
    string Category,
    string? StatusConfiguration,
    string? AllowedPositions,
    string? DefaultNotes,
    string[]? Tags,
    bool IsRequiredByDefault
);
