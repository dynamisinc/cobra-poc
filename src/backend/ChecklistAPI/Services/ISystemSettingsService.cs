/**
 * System Settings Service Interface
 *
 * Manages customer-level configuration settings.
 */

using ChecklistAPI.Models.DTOs;
using ChecklistAPI.Models.Entities;

namespace ChecklistAPI.Services;

public interface ISystemSettingsService
{
    /// <summary>
    /// Get all settings, optionally filtered by category
    /// </summary>
    Task<List<SystemSettingDto>> GetAllSettingsAsync(SettingCategory? category = null);

    /// <summary>
    /// Get a setting by its key
    /// </summary>
    Task<SystemSettingDto?> GetSettingByKeyAsync(string key);

    /// <summary>
    /// Get the raw value of a setting (unmasked)
    /// </summary>
    Task<string?> GetSettingValueAsync(string key);

    /// <summary>
    /// Create a new setting
    /// </summary>
    Task<SystemSettingDto> CreateSettingAsync(CreateSystemSettingRequest request, string modifiedBy);

    /// <summary>
    /// Update an existing setting
    /// </summary>
    Task<SystemSettingDto?> UpdateSettingAsync(string key, UpdateSystemSettingRequest request, string modifiedBy);

    /// <summary>
    /// Update just the value of a setting
    /// </summary>
    Task<SystemSettingDto?> UpdateSettingValueAsync(string key, string value, string modifiedBy);

    /// <summary>
    /// Delete a setting
    /// </summary>
    Task<bool> DeleteSettingAsync(string key);

    /// <summary>
    /// Check if a setting exists and has a non-empty value
    /// </summary>
    Task<bool> HasSettingAsync(string key);

    /// <summary>
    /// Initialize default settings if they don't exist
    /// </summary>
    Task InitializeDefaultSettingsAsync(string modifiedBy);
}
