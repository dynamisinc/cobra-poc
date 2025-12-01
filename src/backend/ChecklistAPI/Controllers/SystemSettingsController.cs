/**
 * System Settings Controller
 *
 * API endpoints for managing customer-level configuration settings.
 * Requires SysAdmin authentication for all operations.
 */

using Microsoft.AspNetCore.Mvc;
using ChecklistAPI.Models.DTOs;
using ChecklistAPI.Models.Entities;
using ChecklistAPI.Services;

namespace ChecklistAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemSettingsController : ControllerBase
{
    private readonly ISystemSettingsService _settingsService;
    private readonly ILogger<SystemSettingsController> _logger;

    public SystemSettingsController(
        ISystemSettingsService settingsService,
        ILogger<SystemSettingsController> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }

    /// <summary>
    /// Get all system settings (values masked for secrets)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SystemSettingDto>>> GetAllSettings(
        [FromQuery] SettingCategory? category = null)
    {
        // In production, verify SysAdmin authentication here
        var settings = await _settingsService.GetAllSettingsAsync(category);
        return Ok(settings);
    }

    /// <summary>
    /// Get a specific setting by key
    /// </summary>
    [HttpGet("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SystemSettingDto>> GetSetting(string key)
    {
        var setting = await _settingsService.GetSettingByKeyAsync(key);
        if (setting == null)
        {
            return NotFound(new { message = $"Setting '{key}' not found" });
        }
        return Ok(setting);
    }

    /// <summary>
    /// Create a new system setting
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SystemSettingDto>> CreateSetting(
        [FromBody] CreateSystemSettingRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var modifiedBy = GetCurrentUser();

        try
        {
            var setting = await _settingsService.CreateSettingAsync(request, modifiedBy);
            return CreatedAtAction(nameof(GetSetting), new { key = setting.Key }, setting);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update a system setting
    /// </summary>
    [HttpPut("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SystemSettingDto>> UpdateSetting(
        string key,
        [FromBody] UpdateSystemSettingRequest request)
    {
        var modifiedBy = GetCurrentUser();
        var setting = await _settingsService.UpdateSettingAsync(key, request, modifiedBy);

        if (setting == null)
        {
            return NotFound(new { message = $"Setting '{key}' not found" });
        }

        return Ok(setting);
    }

    /// <summary>
    /// Update just the value of a setting
    /// </summary>
    [HttpPatch("{key}/value")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SystemSettingDto>> UpdateSettingValue(
        string key,
        [FromBody] UpdateSettingValueRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var modifiedBy = GetCurrentUser();
        var setting = await _settingsService.UpdateSettingValueAsync(key, request.Value, modifiedBy);

        if (setting == null)
        {
            return NotFound(new { message = $"Setting '{key}' not found" });
        }

        return Ok(setting);
    }

    /// <summary>
    /// Delete a system setting
    /// </summary>
    [HttpDelete("{key}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSetting(string key)
    {
        var deleted = await _settingsService.DeleteSettingAsync(key);

        if (!deleted)
        {
            return NotFound(new { message = $"Setting '{key}' not found" });
        }

        return NoContent();
    }

    /// <summary>
    /// Initialize default settings (creates settings if they don't exist)
    /// </summary>
    [HttpPost("initialize")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> InitializeDefaults()
    {
        var modifiedBy = GetCurrentUser();
        await _settingsService.InitializeDefaultSettingsAsync(modifiedBy);
        return Ok(new { message = "Default settings initialized" });
    }

    /// <summary>
    /// Toggle a setting's enabled state
    /// </summary>
    [HttpPatch("{key}/toggle")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SystemSettingDto>> ToggleSetting(string key)
    {
        var existing = await _settingsService.GetSettingByKeyAsync(key);
        if (existing == null)
        {
            return NotFound(new { message = $"Setting '{key}' not found" });
        }

        var modifiedBy = GetCurrentUser();
        var setting = await _settingsService.UpdateSettingAsync(
            key,
            new UpdateSystemSettingRequest { IsEnabled = !existing.IsEnabled },
            modifiedBy);

        return Ok(setting);
    }

    private string GetCurrentUser()
    {
        // Get user from context (set by MockUserMiddleware)
        var userContext = HttpContext.Items["UserContext"] as Models.UserContext;
        return userContext?.Email ?? "system";
    }
}
