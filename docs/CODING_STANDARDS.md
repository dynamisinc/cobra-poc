# Coding Standards & Conventions

> Comprehensive guide for maintaining consistent, high-quality code across the C5 Seeder project.

## Table of Contents
1. [General Principles](#general-principles)
2. [TypeScript/React Standards](#typescriptreact-standards)
3. [C# Standards](#c-standards)
4. [Database Standards](#database-standards)
5. [API Design](#api-design)
6. [Security Guidelines](#security-guidelines)
7. [Testing Standards](#testing-standards)
8. [Documentation Requirements](#documentation-requirements)

---

## General Principles

### Code Quality Fundamentals

**1. Readability First**
- Code is read 10x more than it's written
- Self-documenting code > excessive comments
- Clear variable/function names > clever code
- Simple solutions > complex optimizations (unless proven necessary)

**2. Separation of Concerns**
- Frontend: UI components separate from business logic
- Backend: HTTP handlers separate from service layer
- Services separate from data access
- Each layer has clear responsibilities
- Separate project to hold models and datacontext, referenced by service layer

**3. DRY (Don't Repeat Yourself)**
- Extract repeated logic into functions/utilities
- Create reusable components
- Use shared types/interfaces
- Avoid copy-paste programming

**4. YAGNI (You Aren't Gonna Need It)**
- Build what's needed now, not what might be needed
- Avoid premature abstraction
- Refactor when patterns emerge, not before

**5. Function/Method Size**
- Target: < 50 lines
- If longer, consider breaking into smaller functions
- Each function should do one thing well
- Use descriptive names that explain what the function does

---

## TypeScript/React Standards

### File Organization

```
src/
├── components/
│   ├── common/           # Shared UI components
│   ├── servers/          # Server-specific components
│   └── [feature]/        # Feature-specific components
├── pages/                # Top-level page components
├── services/             # API client services
├── models/               # TypeScript interfaces/types
├── contexts/             # React contexts
├── hooks/                # Custom hooks
├── utils/                # Utility functions
└── constants/            # Application constants
```

### Naming Conventions

| Type | Convention | Example |
|------|-----------|---------|
| Components | PascalCase | `ServerList.tsx` |
| Component Files | Match component name | `ServerList.tsx` |
| Functions/Variables | camelCase | `getUserById` |
| Custom Hooks | camelCase with `use` prefix | `useAuth.ts` |
| Services | camelCase with Service suffix | `serverService.ts` |
| Interfaces | PascalCase, no `I` prefix | `Server`, not `IServer` |
| Types | PascalCase | `ServerType` |
| Enums | PascalCase | `ExecutionStatus` |
| Constants | UPPER_SNAKE_CASE | `API_BASE_URL` |

### Component Structure

```typescript
/**
 * ServerList Component
 * 
 * Displays a paginated list of COBRA servers with filtering and actions.
 * 
 * Features:
 * - Real-time search/filter by name or environment
 * - Pagination with configurable page size
 * - Edit/Delete actions with confirmation dialogs
 * - Loading and error states
 * - Responsive layout (mobile/desktop)
 * 
 * @component
 * @example
 * <ServerList onEdit={handleEdit} onDelete={handleDelete} />
 */

import React, { useState, useEffect } from 'react';
import { Box, Button, CircularProgress } from '@mui/material';
import { Server } from '../models/Server';
import { serverService } from '../services/serverService';

interface ServerListProps {
  /** Callback when edit button is clicked */
  onEdit: (serverId: number) => void;
  /** Callback when delete is confirmed */
  onDelete: (serverId: number) => void;
  /** Optional filter for environment type */
  environmentFilter?: string;
}

export const ServerList: React.FC<ServerListProps> = ({ 
  onEdit, 
  onDelete,
  environmentFilter 
}) => {
  const [servers, setServers] = useState<Server[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadServers();
  }, [environmentFilter]);

  const loadServers = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await serverService.getServers(environmentFilter);
      setServers(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load servers');
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <CircularProgress />;
  if (error) return <Box color="error.main">{error}</Box>;

  return (
    <Box>
      {/* Component JSX */}
    </Box>
  );
};
```

### TypeScript Best Practices

**1. Type Safety**
```typescript
// ✅ Good - Explicit types
interface User {
  id: number;
  username: string;
  email: string;
}

function getUser(id: number): Promise<User> {
  // Implementation
}

// ❌ Bad - Using 'any'
function getUser(id: any): any {
  // Implementation
}
```

**2. Prefer Interfaces for Objects**
```typescript
// ✅ Good
interface ServerDto {
  id: number;
  name: string;
  baseUrl: string;
}

// Use type for unions, primitives, or utilities
type Status = 'pending' | 'success' | 'failed';
type Nullable<T> = T | null;
```

**3. Props with Default Values**
```typescript
interface ComponentProps {
  required: string;
  optional?: number;
}

const Component: React.FC<ComponentProps> = ({ 
  required, 
  optional = 10  // Default value
}) => {
  // Implementation
};
```

**4. Async/Await Error Handling**
```typescript
// ✅ Good
const loadData = async () => {
  try {
    setLoading(true);
    const data = await api.getData();
    setData(data);
  } catch (error) {
    console.error('Failed to load data:', error);
    setError(error instanceof Error ? error.message : 'Unknown error');
  } finally {
    setLoading(false);
  }
};

// ❌ Bad - No error handling
const loadData = async () => {
  const data = await api.getData();
  setData(data);
};
```

### React Patterns

**1. State Management**
```typescript
// Local state - use useState
const [count, setCount] = useState(0);

// Shared state - use Context
const { user } = useAuth();

// Server state - consider React Query (future)
const { data, isLoading, error } = useQuery('servers', fetchServers);
```

**2. Custom Hooks**
```typescript
/**
 * useServers Hook
 * 
 * Manages server data fetching, caching, and mutations.
 * 
 * @returns Server data, loading state, and CRUD operations
 */
function useServers() {
  const [servers, setServers] = useState<Server[]>([]);
  const [loading, setLoading] = useState(false);

  const loadServers = async () => {
    // Implementation
  };

  const createServer = async (dto: CreateServerDto) => {
    // Implementation
  };

  return { servers, loading, loadServers, createServer };
}
```

**3. Effect Cleanup**
```typescript
useEffect(() => {
  let mounted = true;

  const loadData = async () => {
    const data = await api.getData();
    if (mounted) {
      setData(data);
    }
  };

  loadData();

  return () => {
    mounted = false; // Cleanup
  };
}, []);
```

### Material UI Styling

**1. Use sx Prop**
```typescript
// ✅ Good
<Box 
  sx={{ 
    p: 2, 
    bgcolor: 'background.paper',
    borderRadius: 1 
  }}
>
  Content
</Box>

// ❌ Bad - Inline styles
<div style={{ padding: '16px', backgroundColor: '#fff' }}>
  Content
</div>
```

**2. Leverage Theme**
```typescript
import { useTheme } from '@mui/material';

const Component = () => {
  const theme = useTheme();
  
  return (
    <Box sx={{ 
      color: theme.palette.primary.main,
      [theme.breakpoints.down('sm')]: {
        fontSize: '0.875rem'
      }
    }}>
      Content
    </Box>
  );
};
```

---

## C# Standards

### File Organization

```
src/
├── Functions/              # HTTP-triggered functions
├── Services/               # Business logic
│   ├── Interfaces/        # Service interfaces
│   └── Implementations/   # Service implementations
├── Models/
│   ├── DTOs/              # API contracts
│   ├── Entities/          # Database entities
│   └── Requests/          # Request models
├── Data/                   # EF Core
│   ├── AppDbContext.cs
│   ├── Configurations/    # Entity configurations
│   └── Migrations/
└── Utilities/              # Helpers
```

### Naming Conventions

| Type | Convention | Example |
|------|-----------|---------|
| Classes | PascalCase | `ServerService` |
| Interfaces | `I` prefix + PascalCase | `IServerService` |
| Methods | PascalCase | `GetServerByIdAsync` |
| Parameters | camelCase | `serverId` |
| Local Variables | camelCase | `serverDto` |
| Private Fields | `_` prefix + camelCase | `_logger` |
| Constants | PascalCase | `MaxPageSize` |
| Async Methods | `Async` suffix | `CreateServerAsync` |

### Class Structure

```csharp
/// <summary>
/// Service for managing COBRA server configurations.
/// Handles CRUD operations, validation, and business rules.
/// </summary>
public class ServerService : IServerService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ServerService> _logger;
    private readonly IValidator<CreateServerDto> _createValidator;

    /// <summary>
    /// Initializes a new instance of the ServerService class.
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="createValidator">Validator for create operations</param>
    public ServerService(
        AppDbContext context,
        ILogger<ServerService> logger,
        IValidator<CreateServerDto> createValidator)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
    }

    /// <summary>
    /// Retrieves all active servers.
    /// </summary>
    /// <returns>Collection of server DTOs</returns>
    public async Task<IEnumerable<ServerDto>> GetAllServersAsync()
    {
        _logger.LogInformation("Retrieving all active servers");

        var servers = await _context.Servers
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} servers", servers.Count);

        return servers.Select(s => s.ToDto());
    }

    /// <summary>
    /// Creates a new server configuration.
    /// </summary>
    /// <param name="dto">Server creation data</param>
    /// <returns>Created server DTO</returns>
    /// <exception cref="ValidationException">Thrown when validation fails</exception>
    public async Task<ServerDto> CreateServerAsync(CreateServerDto dto)
    {
        _logger.LogInformation("Creating new server: {Name}", dto.Name);

        // Validate
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Check for duplicates
        var exists = await _context.Servers
            .AnyAsync(s => s.Name == dto.Name);

        if (exists)
        {
            throw new InvalidOperationException($"Server with name '{dto.Name}' already exists");
        }

        // Create entity
        var server = new Server
        {
            Name = dto.Name,
            BaseUrl = dto.BaseUrl,
            Environment = dto.Environment,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Servers.Add(server);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created server {ServerId}: {Name}", server.Id, server.Name);

        return server.ToDto();
    }
}
```

### C# Best Practices

**1. Async/Await**
```csharp
// ✅ Good
public async Task<Server> GetServerAsync(int id)
{
    return await _context.Servers
        .FirstOrDefaultAsync(s => s.Id == id);
}

// ❌ Bad - Synchronous I/O
public Server GetServer(int id)
{
    return _context.Servers
        .FirstOrDefault(s => s.Id == id);
}
```

**2. Null Handling**
```csharp
// ✅ Good - Explicit null check
public async Task<ServerDto?> GetServerByIdAsync(int id)
{
    var server = await _context.Servers
        .FirstOrDefaultAsync(s => s.Id == id);

    return server?.ToDto();
}

// For required parameters
public void ProcessServer(Server server)
{
    ArgumentNullException.ThrowIfNull(server);
    // Process
}
```

**3. Using Statements**
```csharp
// ✅ Good - Automatic disposal
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    // Operations
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

**4. LINQ Queries**
```csharp
// ✅ Good - Efficient query
var servers = await _context.Servers
    .Where(s => s.IsActive)
    .OrderBy(s => s.Name)
    .Select(s => new ServerDto
    {
        Id = s.Id,
        Name = s.Name,
        BaseUrl = s.BaseUrl
    })
    .ToListAsync();

// ❌ Bad - Loads entire entity then filters
var allServers = await _context.Servers.ToListAsync();
var activeServers = allServers.Where(s => s.IsActive).ToList();
```

**5. Logging**
```csharp
// ✅ Good - Structured logging
_logger.LogInformation(
    "Server {ServerId} updated by {UserId}", 
    serverId, 
    userId
);

// ✅ Good - Error logging with exception
_logger.LogError(
    ex, 
    "Failed to create server {ServerName}", 
    serverName
);

// ❌ Bad - String interpolation in log message
_logger.LogInformation($"Server {serverId} updated"); // Prevents structured logging
```

---

## Database Standards

### Entity Design

```csharp
/// <summary>
/// Represents a COBRA server configuration
/// </summary>
public class Server
{
    /// <summary>
    /// Unique identifier for the server
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Display name of the server (e.g., "C5 Dev", "Customer QA")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for the server API
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Environment type (Dev, QA, Prod, Customer)
    /// </summary>
    public string Environment { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the server is active and available for use
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp when the server was created (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the server was last updated (UTC)
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<DemoUser> DemoUsers { get; set; } = new List<DemoUser>();
    public ICollection<Execution> Executions { get; set; } = new List<Execution>();
}
```

### Entity Configuration

```csharp
public class ServerConfiguration : IEntityTypeConfiguration<Server>
{
    public void Configure(EntityTypeBuilder<Server> builder)
    {
        // Table
        builder.ToTable("Servers");

        // Primary key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.BaseUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Environment)
            .IsRequired()
            .HasMaxLength(50);

        // Indexes
        builder.HasIndex(e => e.Name)
            .IsUnique();

        builder.HasIndex(e => e.Environment);

        builder.HasIndex(e => e.IsActive);

        // Relationships
        builder.HasMany(e => e.DemoUsers)
            .WithOne(e => e.Server)
            .HasForeignKey(e => e.ServerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

### Migration Standards

**Naming Convention:**
```
YYYYMMDD_HHmmss_DescriptiveName

Examples:
20241106_143000_InitialCreate
20241107_091500_AddDemoUsersTable
20241108_160000_AddExecutionIndexes
```

**Migration Best Practices:**
- One logical change per migration
- Test Up and Down methods
- Include index creation
- Document breaking changes in comments
- Review generated SQL before applying

---

## API Design

### REST Endpoint Conventions

```
GET    /api/servers              List all servers
GET    /api/servers/{id}         Get server by ID
POST   /api/servers              Create new server
PUT    /api/servers/{id}         Update server
DELETE /api/servers/{id}         Delete server

GET    /api/servers/{id}/users   Get users for server
POST   /api/servers/{id}/users   Add user to server
```

### Response Structure

```csharp
/// <summary>
/// Standard API response wrapper
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string>? ValidationErrors { get; set; }

    public static ApiResponse<T> SuccessResult(T data) => new()
    {
        Success = true,
        Data = data
    };

    public static ApiResponse<T> ErrorResult(string message) => new()
    {
        Success = false,
        ErrorMessage = message
    };

    public static ApiResponse<T> ValidationErrorResult(List<string> errors) => new()
    {
        Success = false,
        ValidationErrors = errors
    };
}
```

### HTTP Status Codes

| Code | Usage |
|------|-------|
| 200 OK | Successful GET, PUT, DELETE |
| 201 Created | Successful POST |
| 204 No Content | Successful DELETE with no response body |
| 400 Bad Request | Validation errors, malformed request |
| 401 Unauthorized | Authentication required |
| 403 Forbidden | Authenticated but not authorized |
| 404 Not Found | Resource doesn't exist |
| 409 Conflict | Duplicate or conflicting state |
| 500 Internal Server Error | Unexpected server error |

---

## Security Guidelines

### Input Validation

**Frontend:**
- Validate before sending to API
- Use React Hook Form with validation schemas
- Provide immediate user feedback
- Sanitize user input for display

**Backend:**
- Always validate on server (never trust client)
- Use FluentValidation for complex rules
- Return specific validation errors
- Log validation failures

### Sensitive Data Handling

**DO:**
- ✅ Encrypt passwords using Data Protection API
- ✅ Store encryption keys in Key Vault
- ✅ Use HTTPS for all communication
- ✅ Clear sensitive data from memory after use
- ✅ Use Managed Identity for Azure authentication

**DON'T:**
- ❌ Log passwords, tokens, or API keys
- ❌ Display passwords in UI (even masked)
- ❌ Store secrets in code or config files
- ❌ Return sensitive data in error messages
- ❌ Use weak encryption or custom crypto

### Authentication Patterns

```csharp
// Decrypt C5 password only when needed
public async Task<string> GetBearerTokenAsync(int demoUserId)
{
    var user = await _context.DemoUsers.FindAsync(demoUserId);
    if (user == null) throw new NotFoundException();

    // Decrypt password
    var password = _encryptionService.Decrypt(user.PasswordEncrypted);

    try
    {
        // Use immediately
        var token = await _c5ApiService.AuthenticateAsync(user.Username, password);
        return token;
    }
    finally
    {
        // Clear from memory
        password = null;
    }
}
```

---

## Testing Standards

### Unit Test Structure

```csharp
public class ServerServiceTests
{
    private readonly Mock<AppDbContext> _mockContext;
    private readonly Mock<ILogger<ServerService>> _mockLogger;
    private readonly ServerService _service;

    public ServerServiceTests()
    {
        _mockContext = new Mock<AppDbContext>();
        _mockLogger = new Mock<ILogger<ServerService>>();
        _service = new ServerService(_mockContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllServersAsync_ReturnsActiveServers()
    {
        // Arrange
        var servers = new List<Server>
        {
            new Server { Id = 1, Name = "Dev", IsActive = true },
            new Server { Id = 2, Name = "QA", IsActive = true },
            new Server { Id = 3, Name = "Old", IsActive = false }
        };
        _mockContext.Setup(x => x.Servers).Returns(GetMockDbSet(servers));

        // Act
        var result = await _service.GetAllServersAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, s => Assert.True(s.IsActive));
    }

    [Fact]
    public async Task CreateServerAsync_WithDuplicateName_ThrowsException()
    {
        // Arrange
        var dto = new CreateServerDto { Name = "Existing Server" };
        _mockContext.Setup(x => x.Servers.AnyAsync(It.IsAny<Expression<Func<Server, bool>>>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateServerAsync(dto)
        );
    }
}
```

### Test Naming Convention

```
MethodName_Scenario_ExpectedResult

Examples:
GetServerById_WithValidId_ReturnsServer
GetServerById_WithInvalidId_ReturnsNull
CreateServer_WithDuplicateName_ThrowsException
DeleteServer_WhenNotFound_ThrowsNotFoundException
```

---

## Documentation Requirements

### Code Comments

**When to Comment:**
- Complex business logic
- Non-obvious algorithms
- Workarounds or hacks
- Public APIs
- Class/interface purpose

**When NOT to Comment:**
- Self-explanatory code
- What the code does (use good names instead)
- Outdated information

**XML Documentation (C#):**
```csharp
/// <summary>
/// Generates a demo scenario using Claude AI based on customer requirements.
/// </summary>
/// <param name="discoveryInput">Customer requirements and pain points</param>
/// <returns>Complete scenario with events, logbooks, and chat messages</returns>
/// <exception cref="ValidationException">Thrown when input validation fails</exception>
/// <exception cref="LlmException">Thrown when LLM generation fails</exception>
public async Task<ScenarioDto> GenerateScenarioAsync(DiscoveryInputDto discoveryInput)
{
    // Implementation
}
```

**JSDoc (TypeScript):**
```typescript
/**
 * Fetches all active servers from the API
 * @returns Promise resolving to array of servers
 * @throws {Error} When API request fails
 */
export async function getServers(): Promise<Server[]> {
  // Implementation
}
```

### README Requirements

Every major component should have a README.md:
- Purpose and responsibilities
- How to use/integrate
- Configuration requirements
- Examples
- Common pitfalls

---

## Code Review Checklist

### Before Submitting PR

- [ ] Code compiles without warnings
- [ ] All tests pass
- [ ] New code has appropriate tests
- [ ] Code follows naming conventions
- [ ] Functions are appropriately sized
- [ ] Error handling is comprehensive
- [ ] Logging is appropriate (no sensitive data)
- [ ] Security best practices followed
- [ ] Documentation updated (if needed)
- [ ] Commit messages are clear
- [ ] PR description explains changes

### Reviewer Checklist

- [ ] Code solves the stated problem
- [ ] Logic is correct and efficient
- [ ] Error cases handled
- [ ] Tests are meaningful
- [ ] Code is maintainable
- [ ] Security implications considered
- [ ] Performance implications acceptable
- [ ] Documentation adequate

---

This document is a living guide. Update it as patterns emerge and the team learns what works best.