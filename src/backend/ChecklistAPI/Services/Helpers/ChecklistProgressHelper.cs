using ChecklistAPI.Data;
using ChecklistAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChecklistAPI.Services.Helpers;

/// <summary>
/// ChecklistProgressHelper - Helper for progress calculation logic
///
/// Purpose:
///   Handles progress tracking calculations for checklists.
///   Extracted from ChecklistService to keep files under 250 lines.
///
/// Single Responsibility:
///   ONLY handles progress calculation.
///   Calculates: ProgressPercentage, CompletedItems, RequiredItemsCompleted.
///
/// Dependencies:
///   - ChecklistDbContext: Database access
///   - ILogger: Logging
///
/// Design Pattern:
///   - Static methods (no state)
///   - Called by ChecklistService and item completion endpoints
///   - Updates entity in-place
///
/// Calculation Rules:
///   - ProgressPercentage = (CompletedItems / TotalItems) * 100
///   - CompletedItems = count of items where IsCompleted == true
///   - RequiredItemsCompleted = count of required items where IsCompleted == true
///
/// Author: Checklist POC Team
/// Last Modified: 2025-11-20
/// </summary>
public static class ChecklistProgressHelper
{
    /// <summary>
    /// Recalculates and updates progress for a checklist
    /// Call this after any item completion status changes
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="checklistId">Checklist to recalculate</param>
    public static async Task RecalculateProgressAsync(
        ChecklistDbContext context,
        ILogger logger,
        Guid checklistId)
    {
        logger.LogInformation(
            "Recalculating progress for checklist {ChecklistId}",
            checklistId);

        var checklist = await context.ChecklistInstances
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == checklistId);

        if (checklist == null)
        {
            logger.LogWarning(
                "Checklist {ChecklistId} not found for progress calculation",
                checklistId);
            return;
        }

        CalculateProgress(checklist);

        await context.SaveChangesAsync();

        logger.LogInformation(
            "Updated progress for checklist {ChecklistId}: {Progress}% ({Completed}/{Total})",
            checklistId,
            checklist.ProgressPercentage,
            checklist.CompletedItems,
            checklist.TotalItems);
    }

    /// <summary>
    /// Calculates progress for a checklist (does not save to database)
    /// Used internally and by creation methods
    /// </summary>
    /// <param name="checklist">Checklist entity to calculate progress for</param>
    public static void CalculateProgress(ChecklistInstance checklist)
    {
        // Calculate progress metrics
        checklist.TotalItems = checklist.Items.Count;
        checklist.CompletedItems = checklist.Items.Count(i => i.IsCompleted == true);
        checklist.RequiredItems = checklist.Items.Count(i => i.IsRequired);
        checklist.RequiredItemsCompleted = checklist.Items.Count(
            i => i.IsRequired && i.IsCompleted == true);

        // Calculate percentage (0-100, rounded to 2 decimal places)
        checklist.ProgressPercentage = checklist.TotalItems > 0
            ? Math.Round((decimal)checklist.CompletedItems / checklist.TotalItems * 100, 2)
            : 0;
    }
}
