# Backend Architecture

> **Last Updated:** 2025-12-01
> **Status:** Active - Enforced on all backend development

## Overview

The backend uses a **layered architecture** with clear separation between API controllers, business logic services, and data access. This structure supports:

- **Tool isolation** - Each POC tool has its own controllers and services
- **Shared infrastructure** - Common functionality (auth, logging, DbContext) is reused
- **Independent evaluation** - Each tool can be assessed for production readiness
- **Easy extraction** - Tools can be promoted to production C5 with minimal refactoring

---

## Directory Structure

```
src/backend/ChecklistAPI/
├── Controllers/                    # API endpoints (one per resource)
│   ├── HealthController.cs         # Health check endpoint
│   ├── EventsController.cs         # Event management (shared)
│   ├── EventCategoriesController.cs
│   ├── TemplatesController.cs      # Checklist tool
│   ├── ChecklistsController.cs     # Checklist tool
│   ├── ItemsController.cs          # Checklist tool
│   ├── ChatController.cs           # Chat tool
│   └── SystemSettingsController.cs # Admin
│
├── Services/                       # Business logic layer
│   ├── ITemplateService.cs         # Checklist tool - interface
│   ├── TemplateService.cs          # Checklist tool - implementation
│   ├── IChecklistService.cs        # Checklist tool
│   ├── ChecklistService.cs
│   ├── IEventService.cs            # Shared - events
│   ├── EventService.cs
│   ├── IChatService.cs             # Chat tool
│   └── ChatService.cs
│
├── Hubs/                           # SignalR real-time hubs
│   └── ChecklistHub.cs             # Checklist tool - real-time updates
│
├── Models/
│   ├── Entities/                   # EF Core database entities
│   │   ├── Template.cs
│   │   ├── TemplateItem.cs
│   │   ├── ChecklistInstance.cs
│   │   ├── ChecklistItem.cs
│   │   ├── Event.cs                # Shared
│   │   ├── EventCategory.cs        # Shared
│   │   └── ChatMessage.cs          # Chat tool
│   │
│   ├── DTOs/                       # Data transfer objects
│   │   ├── TemplateDto.cs
│   │   ├── CreateTemplateRequest.cs
│   │   ├── ChecklistDto.cs
│   │   ├── EventDto.cs             # Shared
│   │   └── ChatMessageDto.cs       # Chat tool
│   │
│   └── UserContext.cs              # Mock user context model
│
├── Data/
│   ├── ChecklistDbContext.cs       # EF Core DbContext (shared by all tools)
│   └── Migrations/                 # Database migrations
│
├── Middleware/
│   ├── MockUserMiddleware.cs       # Simulates authentication for POC
│   └── ExceptionMiddleware.cs      # Global exception handling
│
├── Extensions/
│   └── MiddlewareExtensions.cs     # Clean middleware registration
│
└── Program.cs                      # Application configuration
```

---

## Core Infrastructure

### What's Shared Across All Tools

| Component | Purpose | Location |
|-----------|---------|----------|
| `ChecklistDbContext` | Single database context for all entities | `Data/` |
| `MockUserMiddleware` | Provides user context for audit trails | `Middleware/` |
| `UserContext` | Model for current user info | `Models/` |
| Application Insights | Logging and telemetry | `Program.cs` |
| CORS configuration | Frontend access | `Program.cs` |
| Swagger/OpenAPI | API documentation | `Program.cs` |

### Accessing Core Functionality

**User Context (for audit trails):**
```csharp
// In any controller - get current user from middleware
private UserContext GetUserContext()
{
    if (HttpContext.Items.TryGetValue("UserContext", out var context) &&
        context is UserContext userContext)
    {
        return userContext;
    }
    return new UserContext { Email = "unknown", Position = "Unknown" };
}

// Pass to service for audit
var result = await _service.CreateAsync(request, GetUserContext());
```

**Database Access:**
```csharp
// Inject ChecklistDbContext in service constructor
public class MyToolService : IMyToolService
{
    private readonly ChecklistDbContext _context;
    private readonly ILogger<MyToolService> _logger;

    public MyToolService(
        ChecklistDbContext context,
        ILogger<MyToolService> logger)
    {
        _context = context;
        _logger = logger;
    }
}
```

**Logging:**
```csharp
// Use ILogger<T> for structured logging to Application Insights
_logger.LogInformation("Created {EntityType} {EntityId} by {User}",
    "Template", entity.Id, userContext.Email);
```

---

## Adding a New POC Tool

### Step 1: Define Entities

Create database entities in `Models/Entities/`:

```csharp
// Models/Entities/MyToolItem.cs
namespace ChecklistAPI.Models.Entities;

/// <summary>
/// MyToolItem Entity
///
/// Purpose:
///   Represents a single item in MyTool.
///   Includes audit fields for FEMA compliance.
/// </summary>
public class MyToolItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Foreign keys
    public Guid? EventId { get; set; }  // Optional link to shared Event

    // Audit fields (required for all entities)
    public string CreatedBy { get; set; } = string.Empty;
    public string CreatedByPosition { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? LastModifiedBy { get; set; }
    public string? LastModifiedByPosition { get; set; }
    public DateTime? LastModifiedAt { get; set; }

    // Soft delete (required)
    public bool IsArchived { get; set; } = false;
    public string? ArchivedBy { get; set; }
    public DateTime? ArchivedAt { get; set; }

    // Navigation
    public Event? Event { get; set; }
}
```

### Step 2: Add to DbContext

Update `Data/ChecklistDbContext.cs`:

```csharp
public class ChecklistDbContext : DbContext
{
    // Existing DbSets...
    public DbSet<Template> Templates { get; set; }
    public DbSet<ChecklistInstance> ChecklistInstances { get; set; }

    // Add new tool's DbSet
    public DbSet<MyToolItem> MyToolItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Existing configurations...

        // Configure new entity
        modelBuilder.Entity<MyToolItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.IsArchived);
            entity.HasIndex(e => e.EventId);
        });
    }
}
```

### Step 3: Create Migration

```bash
cd src/backend/ChecklistAPI
dotnet ef migrations add AddMyToolItems
dotnet ef database update
```

### Step 4: Define DTOs

Create DTOs in `Models/DTOs/`:

```csharp
// Models/DTOs/MyToolItemDto.cs
namespace ChecklistAPI.Models.DTOs;

/// <summary>
/// Response DTO for MyToolItem
/// </summary>
public record MyToolItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid? EventId { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

// Models/DTOs/CreateMyToolItemRequest.cs
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Request DTO for creating MyToolItem
/// Note: CreatedBy populated by service from UserContext
/// </summary>
public record CreateMyToolItemRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; init; } = string.Empty;

    public Guid? EventId { get; init; }
}
```

### Step 5: Create Service Interface and Implementation

```csharp
// Services/IMyToolService.cs
namespace ChecklistAPI.Services;

public interface IMyToolService
{
    Task<List<MyToolItemDto>> GetAllAsync();
    Task<MyToolItemDto?> GetByIdAsync(Guid id);
    Task<MyToolItemDto> CreateAsync(CreateMyToolItemRequest request, UserContext user);
    Task<MyToolItemDto?> UpdateAsync(Guid id, UpdateMyToolItemRequest request, UserContext user);
    Task<bool> ArchiveAsync(Guid id, UserContext user);
}

// Services/MyToolService.cs
namespace ChecklistAPI.Services;

/// <summary>
/// MyToolService - Business logic for MyTool
///
/// Purpose:
///   Handles CRUD operations with audit trails.
///   Uses shared DbContext and logging.
/// </summary>
public class MyToolService : IMyToolService
{
    private readonly ChecklistDbContext _context;
    private readonly ILogger<MyToolService> _logger;

    public MyToolService(
        ChecklistDbContext context,
        ILogger<MyToolService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<MyToolItemDto>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all MyToolItems");

        var items = await _context.MyToolItems
            .Where(i => !i.IsArchived)
            .OrderBy(i => i.Name)
            .AsNoTracking()
            .ToListAsync();

        return items.Select(MapToDto).ToList();
    }

    public async Task<MyToolItemDto> CreateAsync(
        CreateMyToolItemRequest request,
        UserContext user)
    {
        _logger.LogInformation("Creating MyToolItem by {User}", user.Email);

        var entity = new MyToolItem
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            EventId = request.EventId,
            CreatedBy = user.Email,
            CreatedByPosition = user.Position,
            CreatedAt = DateTime.UtcNow
        };

        _context.MyToolItems.Add(entity);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created MyToolItem {Id}", entity.Id);
        return MapToDto(entity);
    }

    private static MyToolItemDto MapToDto(MyToolItem entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        EventId = entity.EventId,
        CreatedBy = entity.CreatedBy,
        CreatedAt = entity.CreatedAt
    };
}
```

### Step 6: Create Controller

```csharp
// Controllers/MyToolController.cs
namespace ChecklistAPI.Controllers;

/// <summary>
/// MyToolController - API endpoints for MyTool
///
/// Base Route: /api/mytool
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MyToolController : ControllerBase
{
    private readonly IMyToolService _service;
    private readonly ILogger<MyToolController> _logger;

    public MyToolController(
        IMyToolService service,
        ILogger<MyToolController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MyToolItemDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MyToolItemDto>> Create(
        [FromBody] CreateMyToolItemRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userContext = GetUserContext();
        var item = await _service.CreateAsync(request, userContext);

        return CreatedAtAction(
            nameof(GetById),
            new { id = item.Id },
            item);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MyToolItemDto>> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null)
            return NotFound();
        return Ok(item);
    }

    private UserContext GetUserContext()
    {
        if (HttpContext.Items.TryGetValue("UserContext", out var context) &&
            context is UserContext userContext)
        {
            return userContext;
        }
        return new UserContext { Email = "unknown", Position = "Unknown" };
    }
}
```

### Step 7: Register Service

Update `Program.cs`:

```csharp
// Add with other service registrations
builder.Services.AddScoped<IMyToolService, MyToolService>();
```

### Step 8: Add SignalR Hub (if real-time needed)

```csharp
// Hubs/MyToolHub.cs
using Microsoft.AspNetCore.SignalR;

namespace ChecklistAPI.Hubs;

public class MyToolHub : Hub
{
    public async Task ItemUpdated(Guid itemId)
    {
        await Clients.Others.SendAsync("ItemUpdated", itemId);
    }
}

// Register in Program.cs
app.MapHub<MyToolHub>("/hubs/mytool");
```

---

## Linking to Shared Resources

### Using Events (Shared Entity)

Tools can optionally link to Events for organization:

```csharp
// Entity with Event link
public class MyToolItem
{
    public Guid? EventId { get; set; }
    public Event? Event { get; set; }
}

// Query with Event
var items = await _context.MyToolItems
    .Include(i => i.Event)
    .Where(i => i.EventId == eventId)
    .ToListAsync();
```

### Using Positions (ICS Roles)

For position-based filtering:

```csharp
// Filter by position
var items = await _context.MyToolItems
    .Where(i => i.AssignedPosition == userContext.Position)
    .ToListAsync();
```

---

## Required Patterns

### Audit Fields (MANDATORY)

Every entity must include:
```csharp
// Creation audit
public string CreatedBy { get; set; } = string.Empty;
public string CreatedByPosition { get; set; } = string.Empty;
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

// Modification audit
public string? LastModifiedBy { get; set; }
public string? LastModifiedByPosition { get; set; }
public DateTime? LastModifiedAt { get; set; }

// Soft delete
public bool IsArchived { get; set; } = false;
public string? ArchivedBy { get; set; }
public DateTime? ArchivedAt { get; set; }
```

### Soft Delete (MANDATORY)

Never hard delete. See `docs/SOFT_DELETE_PATTERN.md`.

```csharp
// Archive (soft delete)
public async Task<bool> ArchiveAsync(Guid id, UserContext user)
{
    var entity = await _context.MyToolItems.FindAsync(id);
    if (entity == null) return false;

    entity.IsArchived = true;
    entity.ArchivedBy = user.Email;
    entity.ArchivedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();
    return true;
}

// Always filter archived in queries
.Where(i => !i.IsArchived)
```

### Service Interfaces (MANDATORY)

Every service must have an interface for testability.

### Logging (MANDATORY)

Log all operations:
```csharp
_logger.LogInformation("Action {ActionType} on {EntityType} {EntityId} by {User}",
    "Create", "MyToolItem", entity.Id, userContext.Email);
```

---

## POC Tool Evaluation

When evaluating a tool for production:

### Code Quality Checklist
- [ ] All files under 250 lines
- [ ] Service interfaces defined
- [ ] DTOs separate from entities
- [ ] Audit fields on all entities
- [ ] Soft delete implemented
- [ ] Logging throughout
- [ ] Controller is thin (routing only)

### Production Readiness
- [ ] Unit tests written
- [ ] Integration tests for key flows
- [ ] API documentation complete
- [ ] Error handling comprehensive
- [ ] Performance acceptable

### Extraction Steps
1. Copy entities to production project
2. Add DbSet to production DbContext
3. Create migration
4. Copy DTOs, services, controllers
5. Replace mock auth with production auth
6. Update connection strings

---

## File Size Limits

| File Type | Max Lines | Notes |
|-----------|-----------|-------|
| Controllers | 250 | Thin - routing only |
| Services | 250 | Split if larger |
| DTOs | 100 | Keep simple |
| Entities | 100 | Audit fields included |
| Middleware | 150 | Single responsibility |

---

**Questions?** Refer to:
- `CODING_STANDARDS.md` for code conventions
- `SOFT_DELETE_PATTERN.md` for delete patterns
- `IMPLEMENTATION_SUMMARY.md` for checklist tool example
- `FRONTEND_ARCHITECTURE.md` for frontend structure

**Last Updated:** 2025-12-01
