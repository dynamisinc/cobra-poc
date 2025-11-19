using ChecklistAPI.Models;
using ChecklistAPI.Models.DTOs;
using ChecklistAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChecklistAPI.Controllers;

/// <summary>
/// TemplatesController - API endpoints for template management
///
/// Purpose:
///   Provides RESTful endpoints for CRUD operations on checklist templates.
///   Thin controller pattern: validation and routing only, business logic in service.
///
/// Base Route: /api/templates
///
/// Endpoints:
///   GET    /api/templates              - List all active templates
///   GET    /api/templates/{id}         - Get single template
///   GET    /api/templates/category/{category} - Filter by category
///   POST   /api/templates              - Create new template
///   PUT    /api/templates/{id}         - Update existing template
///   DELETE /api/templates/{id}         - Soft delete (archive) template
///   POST   /api/templates/{id}/duplicate - Duplicate template
///
/// User Context:
///   Automatically injected by MockUserMiddleware (POC)
///   Controllers extract from HttpContext.Items["UserContext"]
///
/// Error Handling:
///   - 400 Bad Request: Validation failures
///   - 404 Not Found: Template doesn't exist
///   - 500 Internal Server Error: Unhandled exceptions (logged to App Insights)
///
/// Author: Checklist POC Team
/// Last Modified: 2025-11-19
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TemplatesController : ControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly ILogger<TemplatesController> _logger;

    public TemplatesController(
        ITemplateService templateService,
        ILogger<TemplatesController> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    /// <summary>
    /// Get all active templates
    /// </summary>
    /// <param name="includeInactive">Include inactive templates (default: false)</param>
    /// <returns>List of templates</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TemplateDto>>> GetTemplates(
        [FromQuery] bool includeInactive = false)
    {
        var templates = await _templateService.GetAllTemplatesAsync(includeInactive);
        return Ok(templates);
    }

    /// <summary>
    /// Get a single template by ID
    /// </summary>
    /// <param name="id">Template GUID</param>
    /// <returns>Template with items</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TemplateDto>> GetTemplate(Guid id)
    {
        var template = await _templateService.GetTemplateByIdAsync(id);

        if (template == null)
        {
            _logger.LogWarning("Template {TemplateId} not found", id);
            return NotFound(new { message = $"Template {id} not found" });
        }

        return Ok(template);
    }

    /// <summary>
    /// Get templates filtered by category
    /// </summary>
    /// <param name="category">Category name (case-insensitive)</param>
    /// <returns>List of templates in category</returns>
    [HttpGet("category/{category}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TemplateDto>>> GetTemplatesByCategory(string category)
    {
        var templates = await _templateService.GetTemplatesByCategoryAsync(category);
        return Ok(templates);
    }

    /// <summary>
    /// Create a new template
    /// </summary>
    /// <param name="request">Template data including items</param>
    /// <returns>Newly created template</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TemplateDto>> CreateTemplate(
        [FromBody] CreateTemplateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Get user context from middleware
        var userContext = GetUserContext();

        var template = await _templateService.CreateTemplateAsync(request, userContext);

        _logger.LogInformation(
            "Template {TemplateId} created by {User}",
            template.Id,
            userContext.Email);

        return CreatedAtAction(
            nameof(GetTemplate),
            new { id = template.Id },
            template);
    }

    /// <summary>
    /// Update an existing template
    /// </summary>
    /// <param name="id">Template ID to update</param>
    /// <param name="request">Updated template data</param>
    /// <returns>Updated template</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TemplateDto>> UpdateTemplate(
        Guid id,
        [FromBody] UpdateTemplateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userContext = GetUserContext();

        var template = await _templateService.UpdateTemplateAsync(id, request, userContext);

        if (template == null)
        {
            _logger.LogWarning("Template {TemplateId} not found for update", id);
            return NotFound(new { message = $"Template {id} not found" });
        }

        _logger.LogInformation(
            "Template {TemplateId} updated by {User}",
            id,
            userContext.Email);

        return Ok(template);
    }

    /// <summary>
    /// Soft delete a template (archive)
    /// </summary>
    /// <param name="id">Template ID to archive</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ArchiveTemplate(Guid id)
    {
        var userContext = GetUserContext();

        var success = await _templateService.ArchiveTemplateAsync(id, userContext);

        if (!success)
        {
            _logger.LogWarning("Template {TemplateId} not found for archiving", id);
            return NotFound(new { message = $"Template {id} not found" });
        }

        _logger.LogInformation(
            "Template {TemplateId} archived by {User}",
            id,
            userContext.Email);

        return NoContent();
    }

    /// <summary>
    /// Duplicate an existing template
    /// </summary>
    /// <param name="id">Template ID to duplicate</param>
    /// <param name="request">New name for duplicate</param>
    /// <returns>Newly created duplicate template</returns>
    [HttpPost("{id:guid}/duplicate")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TemplateDto>> DuplicateTemplate(
        Guid id,
        [FromBody] DuplicateTemplateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NewName))
        {
            return BadRequest(new { message = "New name is required" });
        }

        var userContext = GetUserContext();

        var duplicate = await _templateService.DuplicateTemplateAsync(
            id,
            request.NewName,
            userContext);

        if (duplicate == null)
        {
            _logger.LogWarning("Template {TemplateId} not found for duplication", id);
            return NotFound(new { message = $"Template {id} not found" });
        }

        _logger.LogInformation(
            "Template {TemplateId} duplicated to {NewId} by {User}",
            id,
            duplicate.Id,
            userContext.Email);

        return CreatedAtAction(
            nameof(GetTemplate),
            new { id = duplicate.Id },
            duplicate);
    }

    /// <summary>
    /// Extract UserContext from HttpContext (injected by middleware)
    /// Falls back to default if not found (should never happen in POC)
    /// </summary>
    private UserContext GetUserContext()
    {
        if (HttpContext.Items.TryGetValue("UserContext", out var context) &&
            context is UserContext userContext)
        {
            return userContext;
        }

        // Fallback (should not happen if middleware is configured correctly)
        _logger.LogWarning("UserContext not found in HttpContext, using default");
        return new UserContext
        {
            Email = "unknown@cobra.mil",
            FullName = "Unknown User",
            Position = "Unknown",
            IsAdmin = false
        };
    }
}

/// <summary>
/// Request DTO for duplicating a template
/// </summary>
public record DuplicateTemplateRequest
{
    /// <summary>
    /// New name for the duplicated template
    /// </summary>
    public string NewName { get; init; } = string.Empty;
}
