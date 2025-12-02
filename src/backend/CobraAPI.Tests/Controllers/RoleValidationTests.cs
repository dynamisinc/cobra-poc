using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CobraAPI.Tests.Controllers;

/// <summary>
/// Tests for role-based access control in controllers
/// Validates that readonly users cannot modify data and that manage/contributor roles can
/// </summary>
public class RoleValidationTests
{
    private readonly Mock<IChecklistService> _mockChecklistService;
    private readonly Mock<IChecklistItemService> _mockItemService;
    private readonly Mock<ILogger<ChecklistsController>> _mockChecklistLogger;
    private readonly Mock<ILogger<ChecklistItemsController>> _mockItemLogger;

    public RoleValidationTests()
    {
        _mockChecklistService = new Mock<IChecklistService>();
        _mockItemService = new Mock<IChecklistItemService>();
        _mockChecklistLogger = new Mock<ILogger<ChecklistsController>>();
        _mockItemLogger = new Mock<ILogger<ChecklistItemsController>>();
    }

    #region ChecklistsController Tests

    [Fact]
    public async Task CreateFromTemplate_ReturnsOk_WhenUserIsContributor()
    {
        // Arrange
        var controller = CreateChecklistsController(
            TestUserContextFactory.CreateTestUser(role: PermissionRole.Contributor));
        var request = new CreateFromTemplateRequest
        {
            TemplateId = Guid.NewGuid(),
            Name = "Test Checklist",
            EventId = Guid.NewGuid()
        };
        var expectedDto = new ChecklistInstanceDto { Id = Guid.NewGuid(), Name = "Test Checklist" };
        _mockChecklistService
            .Setup(s => s.CreateFromTemplateAsync(It.IsAny<CreateFromTemplateRequest>(), It.IsAny<UserContext>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await controller.CreateFromTemplate(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.NotNull(createdResult.Value);
    }

    [Fact]
    public async Task CreateFromTemplate_ReturnsOk_WhenUserIsManage()
    {
        // Arrange
        var controller = CreateChecklistsController(
            TestUserContextFactory.CreateManagerUser());
        var request = new CreateFromTemplateRequest
        {
            TemplateId = Guid.NewGuid(),
            Name = "Test Checklist",
            EventId = Guid.NewGuid()
        };
        var expectedDto = new ChecklistInstanceDto { Id = Guid.NewGuid(), Name = "Test Checklist" };
        _mockChecklistService
            .Setup(s => s.CreateFromTemplateAsync(It.IsAny<CreateFromTemplateRequest>(), It.IsAny<UserContext>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await controller.CreateFromTemplate(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.NotNull(createdResult.Value);
    }

    [Fact]
    public async Task CreateFromTemplate_Returns403_WhenUserIsReadonly()
    {
        // Arrange
        var controller = CreateChecklistsController(
            TestUserContextFactory.CreateReadonlyUser());
        var request = new CreateFromTemplateRequest
        {
            TemplateId = Guid.NewGuid(),
            Name = "Test Checklist",
            EventId = Guid.NewGuid()
        };

        // Act
        var result = await controller.CreateFromTemplate(request);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, statusResult.StatusCode);
    }

    [Fact]
    public async Task UpdateChecklist_Returns403_WhenUserIsReadonly()
    {
        // Arrange
        var controller = CreateChecklistsController(
            TestUserContextFactory.CreateReadonlyUser());
        var request = new UpdateChecklistRequest { Name = "Updated Name" };

        // Act
        var result = await controller.UpdateChecklist(Guid.NewGuid(), request);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, statusResult.StatusCode);
    }

    [Fact]
    public async Task UpdateChecklist_ReturnsOk_WhenUserIsContributor()
    {
        // Arrange
        var controller = CreateChecklistsController(
            TestUserContextFactory.CreateTestUser(role: PermissionRole.Contributor));
        var request = new UpdateChecklistRequest { Name = "Updated Name" };
        var expectedDto = new ChecklistInstanceDto { Id = Guid.NewGuid(), Name = "Updated Name" };
        _mockChecklistService
            .Setup(s => s.UpdateChecklistAsync(It.IsAny<Guid>(), It.IsAny<UpdateChecklistRequest>(), It.IsAny<UserContext>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await controller.UpdateChecklist(Guid.NewGuid(), request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task ArchiveChecklist_Returns403_WhenUserIsReadonly()
    {
        // Arrange
        var controller = CreateChecklistsController(
            TestUserContextFactory.CreateReadonlyUser());

        // Act
        var result = await controller.ArchiveChecklist(Guid.NewGuid());

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, statusResult.StatusCode);
    }

    [Fact]
    public async Task CloneChecklist_Returns403_WhenUserIsReadonly()
    {
        // Arrange
        var controller = CreateChecklistsController(
            TestUserContextFactory.CreateReadonlyUser());
        var request = new CloneChecklistRequest { NewName = "Cloned Checklist" };

        // Act
        var result = await controller.CloneChecklist(Guid.NewGuid(), request);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, statusResult.StatusCode);
    }

    #endregion

    #region ChecklistItemsController Tests

    [Fact]
    public async Task UpdateItemCompletion_Returns403_WhenUserIsReadonly()
    {
        // Arrange
        var controller = CreateChecklistItemsController(
            TestUserContextFactory.CreateReadonlyUser());
        var request = new UpdateItemCompletionRequest { IsCompleted = true };

        // Act
        var result = await controller.UpdateItemCompletion(Guid.NewGuid(), Guid.NewGuid(), request);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, statusResult.StatusCode);
    }

    [Fact]
    public async Task UpdateItemCompletion_ReturnsOk_WhenUserIsContributor()
    {
        // Arrange
        var controller = CreateChecklistItemsController(
            TestUserContextFactory.CreateTestUser(role: PermissionRole.Contributor));
        var request = new UpdateItemCompletionRequest { IsCompleted = true };
        var expectedDto = new ChecklistItemDto { Id = Guid.NewGuid(), IsCompleted = true };
        _mockItemService
            .Setup(s => s.UpdateItemCompletionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<UpdateItemCompletionRequest>(), It.IsAny<UserContext>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await controller.UpdateItemCompletion(Guid.NewGuid(), Guid.NewGuid(), request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task UpdateItemStatus_Returns403_WhenUserIsReadonly()
    {
        // Arrange
        var controller = CreateChecklistItemsController(
            TestUserContextFactory.CreateReadonlyUser());
        var request = new UpdateItemStatusRequest { Status = "In Progress" };

        // Act
        var result = await controller.UpdateItemStatus(Guid.NewGuid(), Guid.NewGuid(), request);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, statusResult.StatusCode);
    }

    [Fact]
    public async Task UpdateItemStatus_ReturnsOk_WhenUserIsManage()
    {
        // Arrange
        var controller = CreateChecklistItemsController(
            TestUserContextFactory.CreateManagerUser());
        var request = new UpdateItemStatusRequest { Status = "In Progress" };
        var expectedDto = new ChecklistItemDto { Id = Guid.NewGuid(), CurrentStatus = "In Progress" };
        _mockItemService
            .Setup(s => s.UpdateItemStatusAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<UpdateItemStatusRequest>(), It.IsAny<UserContext>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await controller.UpdateItemStatus(Guid.NewGuid(), Guid.NewGuid(), request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task UpdateItemNotes_Returns403_WhenUserIsReadonly()
    {
        // Arrange
        var controller = CreateChecklistItemsController(
            TestUserContextFactory.CreateReadonlyUser());
        var request = new UpdateItemNotesRequest { Notes = "Test notes" };

        // Act
        var result = await controller.UpdateItemNotes(Guid.NewGuid(), Guid.NewGuid(), request);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, statusResult.StatusCode);
    }

    [Fact]
    public async Task UpdateItemNotes_ReturnsOk_WhenUserIsContributor()
    {
        // Arrange
        var controller = CreateChecklistItemsController(
            TestUserContextFactory.CreateTestUser(role: PermissionRole.Contributor));
        var request = new UpdateItemNotesRequest { Notes = "Test notes" };
        var expectedDto = new ChecklistItemDto { Id = Guid.NewGuid(), Notes = "Test notes" };
        _mockItemService
            .Setup(s => s.UpdateItemNotesAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<UpdateItemNotesRequest>(), It.IsAny<UserContext>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await controller.UpdateItemNotes(Guid.NewGuid(), Guid.NewGuid(), request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult.Value);
    }

    #endregion

    #region UserContext Properties Tests

    [Theory]
    [InlineData(PermissionRole.None, false, false)]
    [InlineData(PermissionRole.Readonly, true, false)]
    [InlineData(PermissionRole.Contributor, false, true)]
    [InlineData(PermissionRole.Manage, false, true)]
    public void UserContext_Properties_AreCorrect(PermissionRole role, bool expectedIsReadonly, bool expectedCanEdit)
    {
        // Arrange
        var user = TestUserContextFactory.CreateTestUser(role: role);

        // Assert
        Assert.Equal(expectedIsReadonly, user.IsReadonly);
        Assert.Equal(expectedCanEdit, user.CanEdit);
    }

    #endregion

    #region Helper Methods

    private ChecklistsController CreateChecklistsController(UserContext userContext)
    {
        var controller = new ChecklistsController(_mockChecklistService.Object, _mockChecklistLogger.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = CreateHttpContextWithUser(userContext)
        };
        return controller;
    }

    private ChecklistItemsController CreateChecklistItemsController(UserContext userContext)
    {
        var controller = new ChecklistItemsController(_mockItemService.Object, _mockItemLogger.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = CreateHttpContextWithUser(userContext)
        };
        return controller;
    }

    private HttpContext CreateHttpContextWithUser(UserContext userContext)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Items["UserContext"] = userContext;
        return httpContext;
    }

    #endregion
}
