using ChecklistAPI.Data;
using ChecklistAPI.Models;
using ChecklistAPI.Models.DTOs;
using ChecklistAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChecklistAPI.Services.Helpers;

/// <summary>
/// ChecklistCreationHelper - Helper for complex checklist creation logic
///
/// Purpose:
///   Handles template instantiation and checklist cloning.
///   Extracted from ChecklistService to keep files under 250 lines.
///
/// Single Responsibility:
///   ONLY handles creation logic (from template or cloning).
///   All other CRUD operations remain in ChecklistService.
///
/// Dependencies:
///   - ChecklistDbContext: Database access
///   - ILogger: Logging
///
/// Design Pattern:
///   - Static methods (no state)
///   - Called by ChecklistService
///   - Returns entities (not DTOs) for service to save
///
/// Author: Checklist POC Team
/// Last Modified: 2025-11-20
/// </summary>
public static class ChecklistCreationHelper
{
    /// <summary>
    /// Creates a checklist instance from a template
    /// Copies all template items to checklist items
    /// </summary>
    public static async Task<ChecklistInstance> CreateFromTemplateAsync(
        ChecklistDbContext context,
        ILogger logger,
        CreateFromTemplateRequest request,
        UserContext userContext)
    {
        logger.LogInformation(
            "Creating checklist from template {TemplateId} by {User}",
            request.TemplateId,
            userContext.Email);

        // Retrieve template with items
        var template = await context.Templates
            .Include(t => t.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId);

        if (template == null || !template.IsActive || template.IsArchived)
        {
            throw new InvalidOperationException(
                $"Template {request.TemplateId} not found or not available");
        }

        // Create checklist instance
        var checklist = new ChecklistInstance
        {
            Id = Guid.NewGuid(),
            Name = request.Name ?? $"{template.Name} - {DateTime.UtcNow:yyyy-MM-dd}",
            TemplateId = template.Id,
            EventId = request.EventId,
            EventName = request.EventName,
            OperationalPeriodId = request.OperationalPeriodId,
            OperationalPeriodName = request.OperationalPeriodName,
            AssignedPositions = request.AssignedPositions,
            CreatedBy = userContext.Email,
            CreatedByPosition = userContext.Position,
            CreatedAt = DateTime.UtcNow
        };

        // Copy items from template
        foreach (var templateItem in template.Items)
        {
            checklist.Items.Add(new ChecklistItem
            {
                Id = Guid.NewGuid(),
                ChecklistInstanceId = checklist.Id,
                TemplateItemId = templateItem.Id,
                ItemText = templateItem.ItemText,
                ItemType = templateItem.ItemType,
                DisplayOrder = templateItem.DisplayOrder,
                IsRequired = templateItem.IsRequired,
                StatusOptions = templateItem.StatusOptions,
                AllowedPositions = templateItem.AllowedPositions,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Initialize progress tracking
        InitializeProgress(checklist);

        logger.LogInformation(
            "Created checklist {ChecklistId} with {ItemCount} items from template {TemplateId}",
            checklist.Id,
            checklist.Items.Count,
            template.Id);

        return checklist;
    }

    /// <summary>
    /// Clones an existing checklist with a new name
    /// Resets all completion status
    /// </summary>
    public static async Task<ChecklistInstance> CloneChecklistAsync(
        ChecklistDbContext context,
        ILogger logger,
        Guid checklistId,
        string newName,
        UserContext userContext)
    {
        logger.LogInformation(
            "Cloning checklist {ChecklistId} as '{NewName}'",
            checklistId,
            newName);

        var original = await context.ChecklistInstances
            .Include(c => c.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == checklistId);

        if (original == null)
        {
            throw new InvalidOperationException(
                $"Checklist {checklistId} not found");
        }

        var clone = new ChecklistInstance
        {
            Id = Guid.NewGuid(),
            Name = newName,
            TemplateId = original.TemplateId,
            EventId = original.EventId,
            EventName = original.EventName,
            OperationalPeriodId = original.OperationalPeriodId,
            OperationalPeriodName = original.OperationalPeriodName,
            AssignedPositions = original.AssignedPositions,
            CreatedBy = userContext.Email,
            CreatedByPosition = userContext.Position,
            CreatedAt = DateTime.UtcNow
        };

        // Copy items (reset completion status)
        foreach (var item in original.Items)
        {
            clone.Items.Add(new ChecklistItem
            {
                Id = Guid.NewGuid(),
                ChecklistInstanceId = clone.Id,
                TemplateItemId = item.TemplateItemId,
                ItemText = item.ItemText,
                ItemType = item.ItemType,
                DisplayOrder = item.DisplayOrder,
                IsRequired = item.IsRequired,
                StatusOptions = item.StatusOptions,
                AllowedPositions = item.AllowedPositions,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Initialize progress tracking
        InitializeProgress(clone);

        logger.LogInformation(
            "Cloned checklist {OriginalId} to {NewId}",
            checklistId,
            clone.Id);

        return clone;
    }

    /// <summary>
    /// Initializes progress tracking fields for a new checklist
    /// </summary>
    private static void InitializeProgress(ChecklistInstance checklist)
    {
        checklist.TotalItems = checklist.Items.Count;
        checklist.CompletedItems = 0;
        checklist.RequiredItems = checklist.Items.Count(i => i.IsRequired);
        checklist.RequiredItemsCompleted = 0;
        checklist.ProgressPercentage = 0;
    }
}
