# GitHub Copilot Instructions - C5 Seeder Project

## Project Overview

**C5 Seeder** is an AI-powered demo data generator for the COBRA C5 emergency management system. The application enables Customer Care advocates to rapidly create customized demonstration environments by leveraging Claude AI for scenario generation and automated data injection via REST APIs.

**Architecture**: Three-tier web application

- **Frontend**: React 18+ with TypeScript, Material UI v7+ (minimum v7), Vite
- **Backend**: .NET 8, Azure Functions v4 (Isolated worker model)
- **Database**: Azure SQL Server with EF Core 8

## Technology Stack

### Frontend

- **React 18+** with TypeScript (strict mode)
- **Material UI v7+** (minimum v7) for components and theming
- **Vite** for build tooling
- **React Router v6** for navigation
- **Axios** for HTTP client
- **React Hook Form** for form handling

### Backend

- **.NET 8** (LTS)
- **Azure Functions v4** (Isolated worker model)
- **Entity Framework Core 8** (Code-first migrations)
- **FluentValidation** for input validation
- **Polly** for resilience (retry, circuit breaker)
- **Data Protection API** for credential encryption

### Infrastructure

- **Azure Static Web App** (frontend hosting)
- **Azure Functions** (serverless API)
- **Azure SQL Database** (data storage)
- **Azure Key Vault** (secrets management)
- **Application Insights** (monitoring)

## Coding Conventions

### TypeScript/React

**Naming:**

- Components: `PascalCase` → `ServerList.tsx`
- Functions/variables: `camelCase` → `getUserById`
- Custom hooks: `use` prefix → `useAuth.ts`
- Services: `camelCase` with `Service` suffix → `serverService.ts`
- Interfaces: `PascalCase`, **NO** `I` prefix → `Server` not `IServer`
- Constants: `UPPER_SNAKE_CASE` → `API_BASE_URL`

**Component Structure:**

```typescript
/**
 * ComponentName
 *
 * Brief description of what this component does.
 *
 * Features:
 * - Feature 1
 * - Feature 2
 */
import React, { useState } from "react";

interface ComponentNameProps {
  propName: type;
  optionalProp?: type;
}

export const ComponentName: React.FC<ComponentNameProps> = ({
  propName,
  optionalProp,
}) => {
  // Component implementation
};
```

**Best Practices:**

- Use functional components with hooks
- Prefer `const` over `let`
- Use async/await over promises
- Always handle loading and error states for async operations
- Destructure props in function signature
- Extract complex logic into custom hooks
- Material UI: Use `sx` prop for styling, leverage theme
- **Material UI v7 Grid API**: Use `size` prop instead of `item` prop:
  - ❌ OLD: `<Grid item xs={12} md={6}>`
  - ✅ NEW: `<Grid size={{ xs: 12, md: 6 }}>`
  - Always remove `item` prop when using `size` prop
- **Avoid `any` type - Use as absolute last resort only**:
  - ❌ BAD: `function process(data: any) {}`
  - ✅ GOOD: `function process(data: ServerDto) {}`
  - ✅ GOOD: `function process<T>(data: T) {}`
  - ✅ ACCEPTABLE: `function process(data: unknown) {}` (when type truly unknown)
  - Only use `any` when unavoidable, with explicit `// eslint-disable-next-line @typescript-eslint/no-explicit-any` and justification comment
  - Prefer: specific types, union types (`string | number`), generics (`<T>`), or `unknown` over `any`

### C# Standards

**Naming:**

- Classes: `PascalCase` → `ServerService`
- Interfaces: `I` prefix → `IServerService`
- Methods: `PascalCase` → `GetServerByIdAsync`
- Parameters/variables: `camelCase`
- Private fields: `_` prefix → `_logger`, `_context`
- Constants: `PascalCase`
- Async methods: `Async` suffix → `GetAllServersAsync`

**Class Structure:**

```csharp
/// <summary>
/// Brief description of what this class does.
/// Handles X, Y, and Z operations.
/// </summary>
public class ClassName : IClassName
{
    private readonly IDependency _dependency;
    private readonly ILogger<ClassName> _logger;

    /// <summary>
    /// Initializes a new instance of ClassName.
    /// </summary>
    public ClassName(
        IDependency dependency,
        ILogger<ClassName> logger)
    {
        _dependency = dependency;
        _logger = logger;
    }

    /// <summary>
    /// Brief description of what this method does.
    /// </summary>
    public async Task<ReturnType> MethodNameAsync(ParameterType parameter)
    {
        // Implementation
    }
}
```

**Best Practices:**

- Interface-first design for services
- Dependency injection for all dependencies
- Async/await for all I/O operations
- Use `ILogger<T>` for logging
- XML documentation comments for public APIs
- Use nullable reference types
- Entity Framework: async methods, `AsNoTracking()` for read-only
- Error handling: Try/catch in service methods, log errors

### Database/EF Core

**Entity Conventions:**

- Primary key: `Id` property
- Foreign keys: `{Entity}Id` (e.g., `ServerId`)
- Timestamps: `CreatedAt`, `UpdatedAt` (UTC)
- Soft delete: `IsActive` or `DeletedAt`

**Migration Naming:**

- Format: `YYYYMMDD_DescriptiveName`
- Example: `20241106_AddDemoUsersTable`

## Project Structure

```
c5-seeder/
├── frontend/                    # React frontend
│   ├── src/
│   │   ├── components/         # UI components by feature
│   │   ├── pages/              # Routed page components
│   │   ├── services/           # API client layer
│   │   ├── models/             # TypeScript interfaces
│   │   ├── contexts/           # React contexts (Auth, etc.)
│   │   ├── hooks/              # Custom hooks
│   │   └── utils/              # Helper functions
│   └── README.md
├── api/                         # .NET Azure Functions
│   ├── src/
│   │   ├── Functions/          # HTTP endpoints
│   │   ├── Services/           # Business logic
│   │   ├── Models/
│   │   │   ├── DTOs/           # Data transfer objects
│   │   │   └── Entities/       # EF Core entities
│   │   ├── Data/               # DbContext and migrations
│   │   └── Utilities/          # Helpers
│   └── README.md
├── database/                    # Schema management
│   └── README.md
├── docs/                        # Planning documentation
│   ├── architecture-overview.md
│   └── user-stories/mvp-user-stories.md
└── reference/console-app/       # ⭐ CRITICAL - C5 API patterns
    ├── src/
    │   ├── models/cobraapi/    # C5 API DTOs
    │   ├── services/           # Service implementations
    │   └── interfaces/         # Service contracts
    └── API_REFERENCE.md
```

## Key Patterns

### Service Layer Pattern

**Purpose**: Encapsulate business logic, separate from HTTP concerns

```csharp
// Interface defines contract
public interface IServerService
{
    Task<IEnumerable<ServerDto>> GetAllServersAsync();
    Task<ServerDto?> GetServerByIdAsync(int id);
    Task<ServerDto> CreateServerAsync(CreateServerDto dto);
}

// Implementation contains business logic
public class ServerService : IServerService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ServerService> _logger;

    // Implementation with error handling, logging, validation
}

// Function uses service
public class ServerFunctions
{
    private readonly IServerService _serverService;

    [Function("GetServers")]
    public async Task<HttpResponseData> GetServers(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        var servers = await _serverService.GetAllServersAsync();
        // Return response
    }
}
```

### DTO Pattern

**Purpose**: Separate internal entities from API contracts

- **Entities**: Internal EF Core models with navigation properties
- **DTOs**: Clean API contracts, no circular references
- **Mapping**: Use extension methods or mapping libraries

### API Response Pattern

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string>? ValidationErrors { get; set; }
}
```

## C5 API Integration (Critical)

⚠️ **IMPORTANT**: When implementing C5 API integration (US-007 through US-011), **always reference** the working code in `reference/console-app/`:

### Key Files to Reference

**Authentication:**

- `reference/console-app/src/services/TokenFetcher.cs`
- `reference/console-app/src/services/TokenManager.cs`

**API Client:**

- `reference/console-app/src/services/CobraApiClient.cs`
- `reference/console-app/src/interfaces/ICobraApiClient.cs`

**Data Models:**

- `reference/console-app/src/models/cobraapi/CobraLogbookEntryRequest.cs`
- `reference/console-app/src/models/cobraapi/CobraLogbook.cs`
- `reference/console-app/src/models/cobraapi/EventDetails.cs`

### C5 API Critical Patterns

**Required Headers:**

```csharp
request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
request.Headers.Add("Eventid", eventId);  // ⚠️ Required for most endpoints
request.Headers.Add("Accept-Language", language);
```

**C5 API Endpoints:**

```
GET  Logbooks/SelectedLogbooks       // Get logbooks for event
GET  Logbooks/EntryTypes             // Get entry types
GET  Organizations/Priorities        // Get priority levels
POST Logbooks/{id}/Entries           // Create logbook entry
GET  Events                          // List all events
GET  Events/{id}                     // Get event details
```

**Logbook Entry Creation:**

```csharp
var request = new CobraLogbookEntryRequest
{
    DateOfOccurrence = DateTime.UtcNow,
    LogbookText = "Entry text",
    LogbookEntryTypeId = entryTypeId,
    PriorityId = priorityId,
    TimeZoneId = timeZoneId,           // Required
    TimeZoneAbbreviation = "EST",      // Required
    UtcOffset = "UTC-05:00",           // Required
    PerformAutomaticTranslations = true,
    ParentId = parentGuid,             // For threaded replies
    LocationPointAsWkt = "POINT(-77.1711 38.8823)"  // WKT format
};
```

**Key Requirements:**

1. **EventId header** is required for most API calls (not just in URL)
2. **Timezone fields** are all required: `timeZoneId`, `timeZoneAbbreviation`, `utcOffset`
3. **Location format** must be WKT (Well-Known Text): `POINT(longitude latitude)`
4. **Bearer tokens** expire and need refresh mechanism
5. **Response parsing**: Entry creation returns string GUID that needs parsing

## Security Requirements

### Mandatory Practices

1. **Never log sensitive data**: passwords, tokens, API keys
2. **Encrypt C5 credentials**: Use Data Protection API
3. **Validate all inputs**: Both client and server side
4. **Use parameterized queries**: EF Core does this automatically
5. **HTTPS only**: Enforce at all layers
6. **Secrets in Key Vault**: Never in code or config files

### Password Handling

- C5 demo user passwords encrypted using Data Protection API
- Encryption keys stored in Azure Key Vault
- Decrypt only when needed, never display
- Use Managed Identity to access Key Vault

### Authentication

- Web app: JWT tokens with 8-hour expiration
- C5 API: Bearer tokens from C5 auth endpoint
- Token caching: In-memory with expiration tracking

## Error Handling

### Frontend

- Try/catch around API calls
- Display user-friendly error messages
- Log errors to console (dev) or App Insights (prod)
- Show loading states during operations
- Provide retry options for transient failures

### Backend

- Try/catch in service methods
- Log errors with context (operation, parameters, stack trace)
- Return meaningful error messages (not stack traces)
- Use appropriate HTTP status codes
- Handle transient failures with retry policies (Polly)

## Testing Requirements

### Unit Tests

- Service layer methods
- Utility functions
- Validation logic
- DTO mapping

### Integration Tests

- Database operations (in-memory or test DB)
- API endpoints (test host)
- External API calls (mocked)

## Common Pitfalls to Avoid

### Frontend

- ❌ Not handling loading states
- ❌ Not handling error states
- ❌ Mutating state directly (use setState/useState)
- ❌ Missing key props in lists
- ❌ Not cleaning up useEffect subscriptions
- ❌ Inline styles instead of MUI theming

### Backend

- ❌ Forgetting async/await
- ❌ Not disposing resources (use `using`)
- ❌ Logging sensitive data
- ❌ Hardcoding connection strings
- ❌ Not validating inputs
- ❌ Synchronous I/O operations

### Database

- ❌ Not using migrations (editing DB directly)
- ❌ Forgetting indexes on foreign keys
- ❌ Not using AsNoTracking for read-only queries
- ❌ N+1 query problems
- ❌ Missing cascading delete configurations

## Important Documentation Files

For detailed information, refer to:

- `.claudecontext` - Complete project context for Claude Code
- `CODING_STANDARDS.md` - Comprehensive coding standards (945 lines)
- `QUICKSTART.md` - Step-by-step implementation guide
- `docs/architecture-overview.md` - Technical architecture
- `docs/user-stories/mvp-user-stories.md` - 14 detailed user stories
- `reference/console-app/API_REFERENCE.md` - Complete C5 API documentation
- `reference/console-app/ARCHITECTURE.md` - Reference app architecture

## Development Workflow

### Git Workflow

1. Pick user story from GitHub Issues
2. Create feature branch: `feature/US-XXX-description`
3. Implement with commits referencing issue
4. **Run all tests and ensure they pass**
5. Create PR referencing issue: `Closes #XXX`

### Testing Workflow

**CRITICAL**: Always run tests before committing code or creating PRs. All tests must pass.

#### Backend API Tests

```powershell
# From repository root
cd api
dotnet build
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~ServerFunctionsTests"

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"
```

#### Frontend Tests

```powershell
# From repository root
cd frontend
npm test

# Run tests in watch mode during development
npm run test:watch

# Run tests with coverage
npm run test:coverage

# For CI environments (sequential execution)
.\run-tests-sequential.ps1
```

#### Test-Driven Development (TDD)

When implementing user stories:

1. **Write tests first** for new functionality
2. Run tests and verify they fail (red)
3. Implement the minimum code to make tests pass (green)
4. Refactor while keeping tests passing
5. Ensure all existing tests still pass

#### Pre-Commit Checklist

- [ ] All backend tests pass (`dotnet test`)
- [ ] All frontend tests pass (`npm test`)
- [ ] Integration tests pass for affected features (if applicable)
- [ ] No TypeScript compilation errors
- [ ] No linting errors
- [ ] Code follows project conventions
- [ ] New functionality has corresponding tests (unit + integration)

#### Test Documentation

- **Frontend**: See `frontend/TESTING.md` for comprehensive testing guide
- **Backend**: Unit tests in `api.Tests/`, organized by layer (Functions, Services, Middleware)
- **Integration**: PowerShell tests in `tests/integration/`, see `tests/integration/README.md`
- **Coverage Analysis**: See `TEST_COVERAGE_ANALYSIS.md` for current coverage and gaps

### Commit Messages

```
feat(US-XXX): Brief description

- Detailed change 1
- Detailed change 2

Closes #XXX (GitHub issue number)
```

## Definition of Done

For any feature or bug fix to be considered complete and ready for merge:

### Code Quality

- [ ] Code follows project conventions (see CODING_STANDARDS.md)
- [ ] All functions/components have verbose header comments
- [ ] Code follows separation of concerns and DRY principles
- [ ] No TypeScript compilation errors
- [ ] No linting errors or warnings
- [ ] Code reviewed by at least one team member

### Testing Requirements

- [ ] **Unit tests written and passing** for all new backend services/functions
- [ ] **Unit tests written and passing** for all new frontend components/services
- [ ] **Integration tests** created or updated if feature involves:
  - New API endpoints
  - Changes to request/response contracts
  - Complex multi-step workflows
  - External service integration (LLM, blob storage, etc.)
- [ ] All existing tests still pass (no regressions)
- [ ] Tests cover edge cases and error scenarios
- [ ] Minimum 70% code coverage for new code
- [ ] Critical paths (auth, encryption, data persistence) have 90%+ coverage

### Integration Testing

- [ ] If new API endpoint, integration test exists in `tests/integration/`
- [ ] If modifying existing endpoint, integration test updated
- [ ] Integration test validates:
  - Happy path with realistic data
  - At least one error scenario
  - Response format and data integrity
  - Authentication/authorization requirements
- [ ] Integration test documented in `tests/integration/README.md`

### Execution

- [ ] Backend tests pass: `dotnet test`
- [ ] Frontend tests pass: `npm test` (or `.\run-tests-sequential.ps1` on Windows)
- [ ] Integration tests pass for affected features
- [ ] Tested in local environment
- [ ] No console errors or warnings in browser

### Documentation

- [ ] User story acceptance criteria met
- [ ] README.md updated if setup/usage changed
- [ ] API documentation updated if endpoints changed
- [ ] Migration guide provided if breaking changes
- [ ] Inline code comments for complex logic

### Reference Materials

- **Testing Guide**: See `TEST_COVERAGE_ANALYSIS.md` for coverage requirements
- **Frontend Testing**: See `frontend/TESTING.md` for test setup and execution
- **Integration Testing**: See `tests/integration/README.md` for integration test patterns
- **Coding Standards**: See `CODING_STANDARDS.md` for detailed conventions

## LLM Integration (Claude AI)

### System Context

```
You are an expert in emergency management and incident command systems (ICS).
You generate realistic demo scenarios for the COBRA C5 platform that showcase
specific features and address customer pain points.
```

### User Prompt Structure

```
Generate a {incident_type} scenario for a {industry} organization.

Customer Requirements:
- Pain points: {pain_points}
- Features to showcase: {features}
- Timeline: {duration}

Output Format: JSON matching the ScenarioDto schema
```

## Sprint Planning

### Sprint 1 (~25 points) - Infrastructure

- US-001: Server Registry (5 pts)
- US-002: Demo User Pool with encryption (8 pts)
- US-003: Organization Selection (5 pts)
- US-012: Simple Login (5 pts)
- US-013: Dashboard (5 pts)

### Sprint 2 (~30 points) - LLM Generation

- US-004: Discovery Form (5 pts)
- US-005: Generate Scenario with LLM (13 pts)
- US-006: Review and Modify Scenario (8 pts)

### Sprint 3 (~30 points) - C5 Integration ⚠️ REFERENCE CONSOLE APP HEAVILY

- US-007: Authenticate to COBRA API (8 pts)
- US-008: Create Event and Positions (8 pts)
- US-009: Create Logbooks and Entries (8 pts)
- US-010: Create Chat Messages (5 pts)
- US-011: Monitor Progress (8 pts)

### Sprint 4 (~15 points) - Polish

- US-014: Execution History (8 pts)
- Error handling improvements
- Documentation

## Success Metrics

- ⏱️ Create demo in < 10 minutes
- ✅ 90% LLM generation success rate
- ✅ < 5% execution failure rate
- 👥 Support 5 concurrent users
- ⚡ < 2 second UI response time

---

**For C5 API Integration**: Always reference `reference/console-app/` - it contains proven patterns for authentication, API calls, and data structures. Do not guess at C5 API contracts.

**For Coding Style**: Follow `CODING_STANDARDS.md` religiously - it has 945 lines of detailed conventions with examples.

**For Architecture**: See `docs/architecture-overview.md` for complete system design and data flow diagrams.
