using ChecklistAPI.Data;
using ChecklistAPI.Models;
using ChecklistAPI.Models.DTOs;
using ChecklistAPI.Models.Entities;
using ChecklistAPI.Services;
using ChecklistAPI.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ChecklistAPI.Tests.Services;

/// <summary>
/// Unit tests for ChecklistItemService
/// Tests item completion, status updates, notes, and permission validation
/// </summary>
public class ChecklistItemServiceTests : IDisposable
{
    private readonly ChecklistDbContext _context;
    private readonly Mock<ILogger<ChecklistItemService>> _mockLogger;
    private readonly ChecklistItemService _service;
    private readonly UserContext _testUser;
    private readonly UserContext _alternateUser;

    public ChecklistItemServiceTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
        _mockLogger = new Mock<ILogger<ChecklistItemService>>();
        _service = new ChecklistItemService(_context, _mockLogger.Object);
        _testUser = TestUserContextFactory.CreateTestUser(); // Position: "Safety Officer"
        _alternateUser = new UserContext
        {
            Email = "ops@cobra.mil",
            FullName = "Operations Chief",
            Position = "Operations Section Chief",
            IsAdmin = false
        };
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region GetItemByIdAsync Tests

    [Fact]
    public async Task GetItemByIdAsync_ReturnsItem_WhenItemExists()
    {
        // Arrange
        var (checklistId, itemId) = await SeedChecklistWithItem();

        // Act
        var result = await _service.GetItemByIdAsync(checklistId, itemId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(itemId, result.Id);
        Assert.Equal(checklistId, result.ChecklistInstanceId);
    }

    [Fact]
    public async Task GetItemByIdAsync_ReturnsNull_WhenItemDoesNotExist()
    {
        // Arrange
        var checklistId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        // Act
        var result = await _service.GetItemByIdAsync(checklistId, itemId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetItemByIdAsync_ReturnsNull_WhenItemExistsButInDifferentChecklist()
    {
        // Arrange
        var (checklistId1, itemId1) = await SeedChecklistWithItem();
        var (checklistId2, _) = await SeedChecklistWithItem();

        // Act
        var result = await _service.GetItemByIdAsync(checklistId2, itemId1);

        // Assert
        Assert.Null(result); // Item exists but not in specified checklist
    }

    #endregion

    #region UpdateItemCompletionAsync Tests

    [Fact]
    public async Task UpdateItemCompletion_MarksItemComplete_WithUserAttribution()
    {
        // Arrange
        var (checklistId, itemId) = await SeedChecklistWithItem(itemType: "checkbox");
        var request = new UpdateItemCompletionRequest
        {
            IsCompleted = true,
            Notes = "Verified all PPE is stored"
        };

        // Act
        var result = await _service.UpdateItemCompletionAsync(checklistId, itemId, request, _testUser);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsCompleted);
        Assert.Equal(_testUser.Email, result.CompletedBy);
        Assert.Equal(_testUser.Position, result.CompletedByPosition);
        Assert.NotNull(result.CompletedAt);
        Assert.Equal("Verified all PPE is stored", result.Notes);
    }

    [Fact]
    public async Task UpdateItemCompletion_MarksItemIncomplete_ClearsCompletionFields()
    {
        // Arrange
        var (checklistId, itemId) = await SeedChecklistWithItem(
            itemType: "checkbox",
            isCompleted: true,
            completedBy: "previous@test.com");

        var request = new UpdateItemCompletionRequest
        {
            IsCompleted = false
        };

        // Act
        var result = await _service.UpdateItemCompletionAsync(checklistId, itemId, request, _testUser);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsCompleted);
        Assert.Null(result.CompletedBy);
        Assert.Null(result.CompletedByPosition);
        Assert.Null(result.CompletedAt);
    }

    [Fact]
    public async Task UpdateItemCompletion_ThrowsInvalidOperationException_ForNonCheckboxItems()
    {
        // Arrange
        var (checklistId, itemId) = await SeedChecklistWithItem(itemType: "status");
        var request = new UpdateItemCompletionRequest { IsCompleted = true };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.UpdateItemCompletionAsync(checklistId, itemId, request, _testUser));
    }

    [Fact]
    public async Task UpdateItemCompletion_ReturnsNull_WhenItemNotFound()
    {
        // Arrange
        var checklistId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var request = new UpdateItemCompletionRequest { IsCompleted = true };

        // Act
        var result = await _service.UpdateItemCompletionAsync(checklistId, itemId, request, _testUser);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateItemCompletion_ThrowsUnauthorizedAccessException_WhenPositionNotAllowed()
    {
        // Arrange
        var (checklistId, itemId) = await SeedChecklistWithItem(
            itemType: "checkbox",
            allowedPositions: "Operations Section Chief,Planning Section Chief");

        var request = new UpdateItemCompletionRequest { IsCompleted = true };

        // Act & Assert - testUser is "Safety Officer", not in allowed list
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.UpdateItemCompletionAsync(checklistId, itemId, request, _testUser));
    }

    [Fact]
    public async Task UpdateItemCompletion_Succeeds_WhenAllowedPositionsIsNull()
    {
        // Arrange
        var (checklistId, itemId) = await SeedChecklistWithItem(
            itemType: "checkbox",
            allowedPositions: null);

        var request = new UpdateItemCompletionRequest { IsCompleted = true };

        // Act
        var result = await _service.UpdateItemCompletionAsync(checklistId, itemId, request, _testUser);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsCompleted);
    }

    [Fact]
    public async Task UpdateItemCompletion_Succeeds_WhenUserPositionInAllowedList()
    {
        // Arrange
        var (checklistId, itemId) = await SeedChecklistWithItem(
            itemType: "checkbox",
            allowedPositions: "Safety Officer,Operations Section Chief");

        var request = new UpdateItemCompletionRequest { IsCompleted = true };

        // Act
        var result = await _service.UpdateItemCompletionAsync(checklistId, itemId, request, _testUser);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsCompleted);
    }

    #endregion

    #region UpdateItemStatusAsync Tests

    [Fact]
    public async Task UpdateItemStatus_UpdatesStatusSuccessfully()
    {
        // Arrange
        var (checklistId, itemId) = await SeedChecklistWithItem(
            itemType: "status",
            statusOptions: "Not Started,In Progress,Complete");

        var request = new UpdateItemStatusRequest
        {
            Status = "In Progress",
            Notes = "Started at 14:30"
        };

        // Act
        var result = await _service.UpdateItemStatusAsync(checklistId, itemId, request, _testUser);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("In Progress", result.CurrentStatus);
        Assert.Equal("Started at 14:30", result.Notes);
        Assert.False(result.IsCompleted); // Not "Complete" yet
    }

    [Fact]
    public async Task UpdateItemStatus_MarksItemComplete_WhenStatusIsComplete()
    {
        // Arrange
        var (checklistId, itemId) = await SeedChecklistWithItem(
            itemType: "status",
            statusOptions: "Not Started,In Progress,Complete");

        var request = new UpdateItemStatusRequest
        {
            Status = "Complete"
        };

        // Act
        var result = await _service.UpdateItemStatusAsync(checklistId, itemId, request, _testUser);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Complete", result.CurrentStatus);
        Assert.True(result.IsCompleted); // Marked as complete
    }

    [Fact]
    public async Task UpdateItemStatus_ThrowsInvalidOperationException_ForInvalidStatus()
    {
        // Arrange
        var (checklistId, itemId) = await SeedChecklistWithItem(
            itemType: "status",
            statusOptions: "Not Started,In Progress,Complete");

        var request = new UpdateItemStatusRequest
        {
            Status = "Invalid Status"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.UpdateItemStatusAsync(checklistId, itemId, request, _testUser));
    }

    [Fact]
    public async Task UpdateItemStatus_ThrowsInvalidOperationException_ForNonStatusItems()
    {
        // Arrange
        var (checklistId, itemId) = await SeedChecklistWithItem(itemType: "checkbox");
        var request = new UpdateItemStatusRequest { Status = "Complete" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.UpdateItemStatusAsync(checklistId, itemId, request, _testUser));
    }

    [Fact]
    public async Task UpdateItemStatus_ReturnsNull_WhenItemNotFound()
    {
        // Arrange
        var checklistId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var request = new UpdateItemStatusRequest { Status = "Complete" };

        // Act
        var result = await _service.UpdateItemStatusAsync(checklistId, itemId, request, _testUser);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateItemStatus_ThrowsUnauthorizedAccessException_WhenPositionNotAllowed()
    {
        // Arrange
        var (checklistId, itemId) = await SeedChecklistWithItem(
            itemType: "status",
            statusOptions: "Not Started,Complete",
            allowedPositions: "Operations Section Chief");

        var request = new UpdateItemStatusRequest { Status = "Complete" };

        // Act & Assert - testUser is "Safety Officer", not in allowed list
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.UpdateItemStatusAsync(checklistId, itemId, request, _testUser));
    }

    #endregion

    #region UpdateItemNotesAsync Tests

    [Fact]
    public async Task UpdateItemNotes_UpdatesNotesSuccessfully()
    {
        // Arrange
        var (checklistId, itemId) = await SeedChecklistWithItem();
        var request = new UpdateItemNotesRequest
        {
            Notes = "Verified with Safety Officer at 14:30"
        };

        // Act
        var result = await _service.UpdateItemNotesAsync(checklistId, itemId, request, _testUser);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Verified with Safety Officer at 14:30", result.Notes);
        Assert.Equal(_testUser.Email, result.LastModifiedBy);
        Assert.Equal(_testUser.Position, result.LastModifiedByPosition);
    }

    [Fact]
    public async Task UpdateItemNotes_ClearsNotes_WhenNotesIsNull()
    {
        // Arrange
        var (checklistId, itemId) = await SeedChecklistWithItem(notes: "Old notes");
        var request = new UpdateItemNotesRequest
        {
            Notes = null
        };

        // Act
        var result = await _service.UpdateItemNotesAsync(checklistId, itemId, request, _testUser);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Notes);
    }

    [Fact]
    public async Task UpdateItemNotes_WorksForBothCheckboxAndStatusItems()
    {
        // Arrange
        var (checklistId1, itemId1) = await SeedChecklistWithItem(itemType: "checkbox");
        var (checklistId2, itemId2) = await SeedChecklistWithItem(itemType: "status");
        var request = new UpdateItemNotesRequest { Notes = "Test notes" };

        // Act
        var result1 = await _service.UpdateItemNotesAsync(checklistId1, itemId1, request, _testUser);
        var result2 = await _service.UpdateItemNotesAsync(checklistId2, itemId2, request, _testUser);

        // Assert
        Assert.NotNull(result1);
        Assert.Equal("Test notes", result1.Notes);
        Assert.NotNull(result2);
        Assert.Equal("Test notes", result2.Notes);
    }

    [Fact]
    public async Task UpdateItemNotes_ReturnsNull_WhenItemNotFound()
    {
        // Arrange
        var checklistId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var request = new UpdateItemNotesRequest { Notes = "Test" };

        // Act
        var result = await _service.UpdateItemNotesAsync(checklistId, itemId, request, _testUser);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateItemNotes_ThrowsUnauthorizedAccessException_WhenPositionNotAllowed()
    {
        // Arrange
        var (checklistId, itemId) = await SeedChecklistWithItem(
            allowedPositions: "Operations Section Chief");

        var request = new UpdateItemNotesRequest { Notes = "Test" };

        // Act & Assert - testUser is "Safety Officer", not in allowed list
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.UpdateItemNotesAsync(checklistId, itemId, request, _testUser));
    }

    #endregion

    #region Helper Methods

    private async Task<(Guid checklistId, Guid itemId)> SeedChecklistWithItem(
        string itemType = "checkbox",
        bool isCompleted = false,
        string? completedBy = null,
        string? allowedPositions = null,
        string? statusOptions = null,
        string? notes = null)
    {
        var checklistId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        var checklist = new ChecklistInstance
        {
            Id = checklistId,
            Name = "Test Checklist",
            TemplateId = Guid.NewGuid(),
            EventId = "Event-001",
            EventName = "Test Event",
            CreatedBy = "test@test.com",
            CreatedByPosition = "Test Position"
        };

        var item = new ChecklistItem
        {
            Id = itemId,
            ChecklistInstanceId = checklistId,
            TemplateItemId = Guid.NewGuid(),
            ItemText = "Test Item",
            ItemType = itemType,
            DisplayOrder = 1,
            IsRequired = false,
            IsCompleted = isCompleted,
            CompletedBy = completedBy,
            AllowedPositions = allowedPositions,
            StatusOptions = statusOptions,
            Notes = notes
        };

        _context.ChecklistInstances.Add(checklist);
        _context.ChecklistItems.Add(item);
        await _context.SaveChangesAsync();

        return (checklistId, itemId);
    }

    #endregion
}
