/**
 * SystemSettings Entity
 *
 * Stores customer-level configuration settings for the COBRA system.
 * This is a key-value store with support for encrypted secrets.
 *
 * Categories:
 * - Integration: External service API tokens (GroupMe, Slack, etc.)
 * - AI: LLM provider configuration
 * - System: General system settings
 *
 * Security:
 * - Secrets (API tokens, passwords) are stored encrypted
 * - Only SysAdmin can view/modify settings
 * - Audit trail via ModifiedBy/ModifiedAt
 */

namespace CobraAPI.Admin.Models.Entities;

public class SystemSetting
{
    public Guid Id { get; set; }

    /// <summary>
    /// Unique key for the setting (e.g., "GroupMe.AccessToken", "OpenAI.ApiKey")
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// The setting value (may be encrypted for secrets)
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Category for grouping settings
    /// </summary>
    public SettingCategory Category { get; set; }

    /// <summary>
    /// Human-readable display name
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Description of what this setting controls
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this setting contains sensitive data (shown masked in UI)
    /// </summary>
    public bool IsSecret { get; set; }

    /// <summary>
    /// Whether this setting is currently enabled/active
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Sort order within category
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Who last modified this setting
    /// </summary>
    public string ModifiedBy { get; set; } = string.Empty;

    /// <summary>
    /// When this setting was last modified
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this setting was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Categories for system settings
/// </summary>
public enum SettingCategory
{
    /// <summary>
    /// External service integrations (GroupMe, Slack, etc.)
    /// </summary>
    Integration = 0,

    /// <summary>
    /// AI/LLM provider settings (OpenAI, Azure OpenAI, etc.)
    /// </summary>
    AI = 1,

    /// <summary>
    /// General system settings
    /// </summary>
    System = 2
}
