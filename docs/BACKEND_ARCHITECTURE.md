# Backend Architecture

> **Last Updated:** 2025-12-02
> **Status:** Active - Enforced on all backend development

## Overview

The backend uses a **modular architecture** organized around tools, with clear separation between Core infrastructure, Shared modules, and Tool-specific code. This structure supports:

- **Tool isolation** - Each POC tool is self-contained and can be extracted independently
- **Shared infrastructure** - Common functionality (auth, logging, DbContext) in Core
- **Easy extraction** - Tools can be promoted to production C5 with minimal refactoring
- **Independent evaluation** - Each tool can be assessed for production readiness

---

## Directory Structure

```
src/backend/CobraAPI/
├── Core/                               # Shared infrastructure
│   ├── Data/
│   │   └── CobraDbContext.cs           # Central database context
│   ├── Models/
│   │   ├── UserContext.cs              # Mock user context (replace with C5 auth)
│   │   ├── StatusOption.cs             # Shared status model
│   │   └── Configuration/
│   │       └── FeatureFlagsConfig.cs   # Feature flag settings
│   ├── Middleware/
│   │   └── MockUserMiddleware.cs       # POC authentication
│   ├── Extensions/
│   │   └── MiddlewareExtensions.cs     # DI helpers
│   └── Services/
│       ├── ISystemSettingsService.cs   # System settings interface
│       └── SystemSettingsService.cs
│
├── Shared/                             # Shared across tools
│   └── Events/                         # Event management (COBRA core concept)
│       ├── Controllers/
│       │   ├── EventsController.cs
│       │   ├── EventCategoriesController.cs
│       │   └── OperationalPeriodsController.cs
│       ├── Models/
│       │   ├── Entities/
│       │   │   ├── Event.cs
│       │   │   ├── EventCategory.cs
│       │   │   └── OperationalPeriod.cs
│       │   └── DTOs/
│       │       ├── EventDto.cs
│       │       └── ...
│       └── Services/
│           ├── IEventService.cs
│           ├── EventService.cs
│           └── ...
│
├── Tools/                              # Tool-specific modules
│   ├── Checklist/                      # Checklist tool (lift as unit)
│   │   ├── Controllers/
│   │   ├── Models/Entities/
│   │   ├── Models/DTOs/
│   │   ├── Services/
│   │   ├── Hubs/
│   │   └── Mappers/
│   │
│   ├── Chat/                           # Chat tool (lift as unit)
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── Services/
│   │   ├── Hubs/
│   │   └── ExternalPlatforms/
│   │
│   └── Analytics/                      # Analytics tool
│       ├── Controllers/
│       ├── Models/
│       └── Services/
│
├── Admin/                              # Admin features
│   ├── Controllers/
│   └── Models/
│
├── Migrations/                         # EF Core migrations
├── GlobalUsings.cs                     # Global using directives
└── Program.cs                          # Application composition root
```

---

## Namespace Conventions

```csharp
// Core infrastructure
namespace CobraAPI.Core.Data;
namespace CobraAPI.Core.Models;
namespace CobraAPI.Core.Services;

// Shared modules
namespace CobraAPI.Shared.Events.Models.Entities;
namespace CobraAPI.Shared.Events.Services;

// Tools
namespace CobraAPI.Tools.Checklist.Controllers;
namespace CobraAPI.Tools.Checklist.Models.Entities;
namespace CobraAPI.Tools.Checklist.Services;

namespace CobraAPI.Tools.Chat.Controllers;
namespace CobraAPI.Tools.Chat.Services;

// Admin
namespace CobraAPI.Admin.Controllers;
namespace CobraAPI.Admin.Models.Entities;
```

`GlobalUsings.cs` provides common imports so files don't need extensive using statements.

---

## Adding a New POC Tool

### Step 1: Create Tool Directory Structure

```bash
mkdir -p Tools/MyTool/Controllers
mkdir -p Tools/MyTool/Models/Entities
mkdir -p Tools/MyTool/Models/DTOs
mkdir -p Tools/MyTool/Services
```

### Step 2: Define Entities

```csharp
// Tools/MyTool/Models/Entities/MyToolItem.cs
namespace CobraAPI.Tools.MyTool.Models.Entities;

public class MyToolItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? EventId { get; set; }

    // Audit fields (required)
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Soft delete (required)
    public bool IsArchived { get; set; } = false;

    // Navigation
    public Event? Event { get; set; }
}
```

### Step 3: Add to DbContext

Update `Core/Data/CobraDbContext.cs`:

```csharp
using CobraAPI.Tools.MyTool.Models.Entities;

// Add DbSet
public DbSet<MyToolItem> MyToolItems { get; set; }

// Add configuration method
private void ConfigureMyToolEntities(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<MyToolItem>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
    });
}
```

### Step 4: Update GlobalUsings.cs

```csharp
global using CobraAPI.Tools.MyTool.Models.Entities;
global using CobraAPI.Tools.MyTool.Services;
```

### Step 5: Create Migration

```bash
cd src/backend/CobraAPI
dotnet ef migrations add AddMyToolItems
dotnet ef database update
```

### Step 6: Create Service and Controller

Follow the pattern in existing tools like `Tools/Checklist/`.

### Step 7: Register in Program.cs

```csharp
builder.Services.AddScoped<IMyToolService, MyToolService>();
```

---

## Extracting a Tool to Production

When lifting a tool into COBRA 5:

1. **Copy the entire tool folder** (`Tools/MyTool/`)
2. **Add DbSet** to production DbContext
3. **Create migration** in production
4. **Register services** in production DI
5. **Replace** mock auth with production authentication

The modular structure means the tool can be copied as a unit with minimal changes.

---

**Related Documentation:**
- `CODING_STANDARDS.md` - Code conventions
- `FRONTEND_ARCHITECTURE.md` - Frontend module structure

**Last Updated:** 2025-12-02
