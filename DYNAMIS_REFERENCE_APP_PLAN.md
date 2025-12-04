# dynamis-reference-app Implementation Plan

> **Purpose:** Create a GitHub template repository for starting new Dynamis applications
> **Source Projects:** cobra-poc (styling, navigation, patterns) + c5-seeder (CI/CD, Azure Functions architecture)

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Phase 1: Repository Setup](#2-phase-1-repository-setup)
3. [Phase 2: Backend Foundation](#3-phase-2-backend-foundation)
4. [Phase 3: Frontend Foundation](#4-phase-3-frontend-foundation)
5. [Phase 4: CI/CD Pipeline](#5-phase-4-cicd-pipeline)
6. [Phase 5: Documentation](#6-phase-5-documentation)
7. [Phase 6: Sample Feature](#7-phase-6-sample-feature)
8. [Phase 7: Testing & Validation](#8-phase-7-testing--validation)
9. [File Structure](#9-file-structure)
10. [Implementation Checklist](#10-implementation-checklist)

---

## 1. Architecture Overview

### Target Architecture

```
┌──────────────────────────────────────┐
│      Azure Static Web App (SWA)      │
│  ┌────────────────────────────────┐  │
│  │        React Frontend          │  │
│  │  - Vite + TypeScript           │  │
│  │  - Material-UI + COBRA Theme   │  │
│  │  - React Router                │  │
│  └────────────────────────────────┘  │
└──────────────────────────────────────┘
                 │ HTTPS
                 ▼
┌──────────────────────────────────────┐
│     Azure Functions (.NET 8+)        │
│  ┌────────────────────────────────┐  │
│  │   Isolated Worker Process      │  │
│  │  - HTTP Triggers (REST API)    │  │
│  │  - EF Core + SQL Server        │  │
│  │  - Structured Logging          │  │
│  │  - Application Insights        │  │
│  └────────────────────────────────┘  │
└──────────────────────────────────────┘
                 │
        ┌────────┴────────┐
        ▼                 ▼
┌───────────────┐  ┌───────────────────┐
│  Azure SQL    │  │ Azure SignalR     │
│  Database     │  │ Service           │
└───────────────┘  └───────────────────┘

┌──────────────────────────────────────┐
│        Application Insights          │
│  (Optional - enabled via config)     │
└──────────────────────────────────────┘
```

### Key Technology Choices

| Layer | Technology | Version |
|-------|------------|---------|
| Frontend Framework | React | 18.x |
| Frontend Build | Vite | 5.x |
| Frontend Language | TypeScript | 5.x |
| UI Library | Material-UI | 6.x or 7.x |
| Styling System | COBRA (custom MUI theme) | - |
| Backend Runtime | Azure Functions | Isolated Worker |
| Backend Framework | .NET | 8.0 |
| ORM | Entity Framework Core | 8.x |
| Database | Azure SQL / SQL Server | 2019+ |
| Real-time | Azure SignalR Service | - |
| Logging | ILogger + Application Insights | - |
| CI/CD | GitHub Actions | - |
| IaC | Bicep (reference/untested) | - |

---

## 2. Phase 1: Repository Setup

### 2.1 Create GitHub Repository

- [ ] Create new repository `dynamisinc/dynamis-reference-app`
- [ ] Configure as GitHub Template repository
- [ ] Set up branch protection rules for `main`
- [ ] Create initial `.gitignore` (combined .NET + Node.js)
- [ ] Create `LICENSE` file
- [ ] Create initial `README.md`

### 2.2 Directory Structure Setup

```
dynamis-reference-app/
├── .github/
│   ├── workflows/
│   ├── ISSUE_TEMPLATE/
│   └── PULL_REQUEST_TEMPLATE.md
├── src/
│   ├── api/                    # Azure Functions backend
│   └── frontend/               # React SPA
├── infrastructure/             # Bicep templates (reference)
├── database/                   # SQL scripts, migrations reference
├── docs/                       # Documentation
├── scripts/                    # Development helper scripts
├── .gitignore
├── .editorconfig
├── README.md
└── VERSION
```

### 2.3 Editor Configuration

- [ ] `.editorconfig` for consistent formatting
- [ ] `.vscode/` settings for recommended extensions
- [ ] `.vscode/launch.json` for debugging both frontend and backend

---

## 3. Phase 2: Backend Foundation

### 3.1 Azure Functions Project Setup

**Location:** `src/api/`

- [ ] Create Azure Functions Isolated Worker project (.NET 8)
- [ ] Configure `host.json` for Functions runtime
- [ ] Configure `local.settings.json` (with `.example` template)

**Project Structure:**
```
src/api/
├── Api.csproj
├── Program.cs                  # DI, logging, middleware setup
├── host.json
├── local.settings.json         # Git-ignored
├── local.settings.example.json # Template for developers
│
├── Core/                       # Shared infrastructure
│   ├── Data/
│   │   └── AppDbContext.cs
│   ├── Logging/
│   │   ├── LoggingExtensions.cs
│   │   └── CorrelationIdMiddleware.cs
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs
│   ├── Telemetry/
│   │   └── ApplicationInsightsSetup.cs
│   └── Extensions/
│       └── ServiceCollectionExtensions.cs
│
├── Shared/                     # Cross-tool shared code
│   └── Health/
│       ├── HealthFunction.cs   # /api/health endpoint
│       └── HealthResponse.cs
│
├── Tools/                      # Tool-specific modules
│   └── Sample/                 # Example tool (can be removed)
│       ├── Functions/
│       │   └── SampleFunction.cs
│       ├── Models/
│       │   ├── Entities/
│       │   │   └── SampleItem.cs
│       │   └── DTOs/
│       │       └── SampleItemDto.cs
│       ├── Services/
│       │   ├── ISampleService.cs
│       │   └── SampleService.cs
│       └── Mappers/
│           └── SampleMapper.cs
│
├── Hubs/                       # SignalR hubs
│   └── NotificationHub.cs
│
└── Migrations/                 # EF Core migrations
```

### 3.2 Logging Infrastructure

**Priority: HIGH** - Robust logging from the start

- [ ] Configure structured logging with ILogger
- [ ] Set up log levels (Debug, Info, Warning, Error, Critical)
- [ ] Implement correlation ID middleware for request tracing
- [ ] Configure Application Insights (optional, feature-flagged)
- [ ] Request/response logging middleware
- [ ] Performance logging for slow operations

**Logging Configuration Pattern:**
```csharp
// Program.cs
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();

    // Optional: Application Insights
    if (configuration.GetValue<bool>("ApplicationInsights:Enabled"))
    {
        logging.AddApplicationInsights();
    }
});
```

### 3.3 Database Setup (EF Core)

- [ ] Configure `AppDbContext` with conventions
- [ ] Create sample entity (`SampleItem`)
- [ ] Set up connection string patterns for local/Azure
- [ ] Initial migration
- [ ] Seed data pattern

### 3.4 SignalR Integration

- [ ] Configure Azure SignalR Service bindings
- [ ] Create `NotificationHub` base class
- [ ] Document local development setup (emulator or Azure resource)
- [ ] Sample broadcast pattern

### 3.5 Health Check Endpoint

- [ ] `/api/health` endpoint with:
  - API status
  - Database connectivity check
  - SignalR connectivity check
  - Version information
  - Environment name

---

## 4. Phase 3: Frontend Foundation

### 4.1 Vite + React + TypeScript Setup

**Location:** `src/frontend/`

- [ ] Initialize Vite project with React + TypeScript template
- [ ] Configure `vite.config.ts` with proxy for local API
- [ ] Configure `tsconfig.json` with strict mode
- [ ] Set up path aliases (`@/core`, `@/shared`, `@/tools`, `@/theme`)

**Project Structure:**
```
src/frontend/
├── public/
│   ├── staticwebapp.config.json  # SWA routing config
│   └── favicon.ico
│
├── src/
│   ├── main.tsx                  # App entry point
│   ├── App.tsx                   # Root component + routing
│   │
│   ├── core/                     # App-wide infrastructure
│   │   ├── components/
│   │   │   ├── navigation/
│   │   │   │   ├── AppLayout.tsx
│   │   │   │   ├── AppHeader.tsx
│   │   │   │   ├── Sidebar.tsx         # LEFT navigation sidebar
│   │   │   │   └── Breadcrumb.tsx
│   │   │   ├── ProfileMenu.tsx         # Minimal profile menu
│   │   │   └── ErrorBoundary.tsx
│   │   ├── services/
│   │   │   └── api.ts                  # Axios client
│   │   ├── hooks/
│   │   │   └── useApi.ts
│   │   └── utils/
│   │       └── logger.ts               # Frontend logging utility
│   │
│   ├── shared/                   # Cross-tool shared code
│   │   ├── hooks/
│   │   │   └── useSignalR.ts
│   │   └── types/
│   │       └── common.ts
│   │
│   ├── tools/                    # Tool-specific modules
│   │   └── sample/               # Example tool (can be removed)
│   │       ├── components/
│   │       ├── pages/
│   │       │   └── SamplePage.tsx
│   │       ├── services/
│   │       │   └── sampleService.ts
│   │       ├── hooks/
│   │       └── types/
│   │
│   ├── theme/                    # COBRA styling system
│   │   ├── cobraTheme.ts         # MUI theme configuration
│   │   ├── CobraStyles.ts        # Spacing/padding constants
│   │   ├── c5Colors.ts           # Color palette
│   │   └── styledComponents/     # Pre-built COBRA components
│   │       ├── index.ts
│   │       ├── CobraButtons.tsx
│   │       ├── CobraInputs.tsx
│   │       └── CobraDialogs.tsx
│   │
│   └── types/                    # Global TypeScript types
│       └── index.ts
│
├── .env.example                  # Environment template
├── .env.development              # Local dev config
├── package.json
├── tsconfig.json
├── vite.config.ts
└── vitest.config.ts
```

### 4.2 COBRA Styling System

**Copy from cobra-poc with adaptations:**

- [ ] `cobraTheme.ts` - Full MUI theme customization
- [ ] `CobraStyles.ts` - Spacing and padding constants
- [ ] `c5Colors.ts` - Color palette definitions
- [ ] Styled components:
  - [ ] Buttons: `CobraPrimaryButton`, `CobraSecondaryButton`, `CobraDeleteButton`, `CobraNewButton`, `CobraSaveButton`, `CobraLinkButton`
  - [ ] Inputs: `CobraTextField`, `CobraCheckbox`, `CobraSwitch`
  - [ ] Layout: `CobraDialog`, `CobraDivider`

### 4.3 Navigation Components

**Left Sidebar Navigation (from cobra-poc):**

- [ ] `AppLayout.tsx` - Main layout wrapper
- [ ] `AppHeader.tsx` - 54px header with app name + profile
- [ ] `Sidebar.tsx` - Collapsible left sidebar (64px closed, 288px open)
- [ ] `Breadcrumb.tsx` - Route-based breadcrumbs
- [ ] `ProfileMenu.tsx` - Minimal version (user info, logout)

**Navigation Features:**
- Collapsible sidebar with smooth animation
- Active route highlighting
- Breadcrumb auto-generation from route structure
- Mobile-responsive behavior

### 4.4 API Service Layer

- [ ] Axios client with interceptors
- [ ] Base URL configuration (env-based)
- [ ] Error handling patterns
- [ ] Request/response logging
- [ ] Type-safe API calls

### 4.5 SignalR Client Integration

- [ ] `useSignalR` hook for connection management
- [ ] Auto-reconnect logic
- [ ] Connection state tracking
- [ ] Sample event subscription pattern

### 4.6 Static Web App Configuration

**`staticwebapp.config.json`:**
```json
{
  "navigationFallback": {
    "rewrite": "/index.html",
    "exclude": ["/assets/*", "/*.{js,css,svg,png,ico,json}"]
  },
  "routes": [
    { "route": "/api/*", "allowedRoles": ["anonymous"] }
  ],
  "globalHeaders": {
    "cache-control": "no-cache, no-store, must-revalidate"
  }
}
```

---

## 5. Phase 4: CI/CD Pipeline

### 5.1 GitHub Actions Workflows

**Main Deployment Workflow:** `.github/workflows/deploy.yml`

```yaml
Jobs:
1. test          - Run backend + frontend tests
2. build         - Build API + Frontend artifacts
3. deploy-api    - Deploy to Azure Functions
4. deploy-frontend - Deploy to Azure SWA
5. validate      - Health checks post-deployment
6. summary       - Deployment summary
```

- [ ] Create `deploy.yml` (adapted from c5-seeder)
- [ ] Create `test.yml` (reusable test workflow)
- [ ] Create `deploy-api-only.yml` (quick API deploy)
- [ ] Configure GitHub environments (dev, staging, prod)
- [ ] Document required GitHub secrets

**Required GitHub Secrets:**
```
AZURE_CREDENTIALS              # Service principal JSON
AZURE_FUNCTION_APP_URL         # Function app base URL
AZURE_STATIC_WEB_APP_URL       # SWA URL
AZURE_SQL_CONNECTION_STRING    # (optional for migrations)
```

### 5.2 Issue Templates

- [ ] `user-story.md` - Template for user stories
- [ ] `bug-report.md` - Bug report template
- [ ] `feature-request.md` - Feature request template

### 5.3 Pull Request Template

- [ ] Checklist for PR reviewers
- [ ] Link to user story requirement
- [ ] Testing verification

---

## 6. Phase 5: Documentation

### 6.1 Core Documentation Files

| File | Purpose | Priority |
|------|---------|----------|
| `README.md` | Project overview, quick start | HIGH |
| `docs/CLAUDE.md` | AI agent instructions | HIGH |
| `docs/CODING_STANDARDS.md` | Code conventions | HIGH |
| `docs/GETTING_STARTED.md` | New developer onboarding | HIGH |
| `docs/DEVELOPMENT_WORKFLOW.md` | User stories → GH issues | HIGH |
| `docs/LOGGING_GUIDE.md` | Logging patterns | MEDIUM |
| `docs/COBRA_STYLING.md` | COBRA styling reference | HIGH |
| `docs/NAVIGATION_CUSTOMIZATION.md` | How to modify/remove nav | MEDIUM |
| `docs/SIGNALR_GUIDE.md` | Real-time integration | MEDIUM |
| `docs/DEPLOYMENT.md` | CI/CD and Azure setup | HIGH |

### 6.2 Architecture Documentation

| File | Purpose |
|------|---------|
| `docs/architecture/OVERVIEW.md` | High-level architecture |
| `docs/architecture/FRONTEND.md` | Frontend structure |
| `docs/architecture/BACKEND.md` | Backend structure |
| `docs/architecture/DATABASE.md` | Data layer patterns |

### 6.3 User Stories Template

**Location:** `docs/features/sample/USER_STORIES.md`

```markdown
# Feature: Sample Feature Name

## Overview
Brief description of the feature.

## User Stories

### US-001: As a user, I want to...
**Description:** ...
**Acceptance Criteria:**
- [ ] Criterion 1
- [ ] Criterion 2

**Technical Notes:**
- Implementation considerations

### US-002: As an admin, I want to...
...
```

### 6.4 CLAUDE.md Structure

Comprehensive AI agent guide including:
- Project overview
- Tech stack
- File structure
- Code conventions
- COBRA styling rules (mandatory)
- Common tasks & workflows
- API patterns
- Testing guidelines
- Deployment process

---

## 7. Phase 6: Sample Feature

### 7.1 Purpose

Provide a complete, working example that demonstrates:
- Backend API endpoint pattern
- Frontend page with COBRA styling
- Service layer integration
- Navigation integration
- SignalR real-time updates

### 7.2 Sample Feature: "Notes"

A simple notes feature that demonstrates all patterns:

**Backend:**
- `GET /api/notes` - List all notes
- `GET /api/notes/{id}` - Get single note
- `POST /api/notes` - Create note
- `PUT /api/notes/{id}` - Update note
- `DELETE /api/notes/{id}` - Delete note
- SignalR: Broadcast when notes change

**Frontend:**
- Notes list page
- Create/Edit note dialog
- Real-time updates when another user modifies

**Database:**
- `Notes` table with basic fields

---

## 8. Phase 7: Testing & Validation

### 8.1 Backend Tests

- [ ] Unit tests for services
- [ ] Integration tests for API endpoints
- [ ] Test project structure mirroring main project

### 8.2 Frontend Tests

- [ ] Component tests with Vitest + React Testing Library
- [ ] Colocated test files (`.test.tsx` next to source)
- [ ] Test utilities and providers

### 8.3 End-to-End Validation

- [ ] Manual deployment to test Azure environment
- [ ] Verify CI/CD pipeline works
- [ ] Validate health checks
- [ ] Test SignalR connectivity
- [ ] Verify SWA routing

---

## 9. File Structure

### Complete Template Structure

```
dynamis-reference-app/
│
├── .github/
│   ├── workflows/
│   │   ├── deploy.yml
│   │   ├── deploy-api-only.yml
│   │   └── test.yml
│   ├── ISSUE_TEMPLATE/
│   │   ├── user-story.md
│   │   ├── bug-report.md
│   │   └── feature-request.md
│   └── PULL_REQUEST_TEMPLATE.md
│
├── src/
│   ├── api/
│   │   ├── Api.csproj
│   │   ├── Program.cs
│   │   ├── host.json
│   │   ├── local.settings.example.json
│   │   ├── Core/
│   │   │   ├── Data/
│   │   │   │   └── AppDbContext.cs
│   │   │   ├── Logging/
│   │   │   │   ├── LoggingExtensions.cs
│   │   │   │   └── CorrelationIdMiddleware.cs
│   │   │   ├── Middleware/
│   │   │   │   └── ExceptionHandlingMiddleware.cs
│   │   │   ├── Telemetry/
│   │   │   │   └── ApplicationInsightsSetup.cs
│   │   │   └── Extensions/
│   │   │       └── ServiceCollectionExtensions.cs
│   │   ├── Shared/
│   │   │   └── Health/
│   │   │       ├── HealthFunction.cs
│   │   │       └── HealthResponse.cs
│   │   ├── Tools/
│   │   │   └── Sample/
│   │   │       ├── Functions/
│   │   │       │   └── NotesFunction.cs
│   │   │       ├── Models/
│   │   │       │   ├── Entities/
│   │   │       │   │   └── Note.cs
│   │   │       │   └── DTOs/
│   │   │       │       ├── NoteDto.cs
│   │   │       │       └── CreateNoteRequest.cs
│   │   │       ├── Services/
│   │   │       │   ├── INotesService.cs
│   │   │       │   └── NotesService.cs
│   │   │       └── Mappers/
│   │   │           └── NoteMapper.cs
│   │   ├── Hubs/
│   │   │   └── NotificationHub.cs
│   │   └── Migrations/
│   │
│   ├── api.Tests/
│   │   ├── Api.Tests.csproj
│   │   ├── Tools/
│   │   │   └── Sample/
│   │   │       └── NotesServiceTests.cs
│   │   └── Helpers/
│   │       └── TestDbContextFactory.cs
│   │
│   └── frontend/
│       ├── public/
│       │   ├── staticwebapp.config.json
│       │   └── favicon.ico
│       ├── src/
│       │   ├── main.tsx
│       │   ├── App.tsx
│       │   ├── core/
│       │   │   ├── components/
│       │   │   │   ├── navigation/
│       │   │   │   │   ├── AppLayout.tsx
│       │   │   │   │   ├── AppHeader.tsx
│       │   │   │   │   ├── Sidebar.tsx
│       │   │   │   │   └── Breadcrumb.tsx
│       │   │   │   ├── ProfileMenu.tsx
│       │   │   │   └── ErrorBoundary.tsx
│       │   │   ├── services/
│       │   │   │   └── api.ts
│       │   │   ├── hooks/
│       │   │   │   └── useApi.ts
│       │   │   └── utils/
│       │   │       └── logger.ts
│       │   ├── shared/
│       │   │   ├── hooks/
│       │   │   │   └── useSignalR.ts
│       │   │   └── types/
│       │   │       └── common.ts
│       │   ├── tools/
│       │   │   └── sample/
│       │   │       ├── components/
│       │   │       │   ├── NoteCard.tsx
│       │   │       │   └── NoteDialog.tsx
│       │   │       ├── pages/
│       │   │       │   └── NotesPage.tsx
│       │   │       ├── services/
│       │   │       │   └── notesService.ts
│       │   │       ├── hooks/
│       │   │       │   └── useNotes.ts
│       │   │       └── types/
│       │   │           └── index.ts
│       │   ├── theme/
│       │   │   ├── cobraTheme.ts
│       │   │   ├── CobraStyles.ts
│       │   │   ├── c5Colors.ts
│       │   │   └── styledComponents/
│       │   │       ├── index.ts
│       │   │       ├── CobraButtons.tsx
│       │   │       ├── CobraInputs.tsx
│       │   │       └── CobraDialogs.tsx
│       │   └── types/
│       │       └── index.ts
│       ├── .env.example
│       ├── package.json
│       ├── tsconfig.json
│       ├── vite.config.ts
│       └── vitest.config.ts
│
├── infrastructure/
│   ├── README.md                 # "Reference only - validate before use"
│   ├── main.bicep
│   └── modules/
│       ├── function-app.bicep
│       ├── static-web-app.bicep
│       ├── sql-server.bicep
│       ├── storage.bicep
│       ├── app-insights.bicep
│       └── signalr.bicep
│
├── database/
│   ├── README.md
│   └── seed-data.sql             # Optional seed data
│
├── scripts/
│   ├── setup-local.ps1           # Local dev setup script
│   ├── run-migrations.ps1        # Apply EF migrations
│   └── create-github-issues.ps1  # Create issues from user stories
│
├── docs/
│   ├── CLAUDE.md
│   ├── CODING_STANDARDS.md
│   ├── GETTING_STARTED.md
│   ├── DEVELOPMENT_WORKFLOW.md
│   ├── LOGGING_GUIDE.md
│   ├── COBRA_STYLING.md
│   ├── NAVIGATION_CUSTOMIZATION.md
│   ├── SIGNALR_GUIDE.md
│   ├── DEPLOYMENT.md
│   ├── architecture/
│   │   ├── OVERVIEW.md
│   │   ├── FRONTEND.md
│   │   ├── BACKEND.md
│   │   └── DATABASE.md
│   └── features/
│       └── sample/
│           └── USER_STORIES.md
│
├── .editorconfig
├── .gitignore
├── LICENSE
├── README.md
└── VERSION
```

---

## 10. Implementation Checklist

### Phase 1: Repository Setup
- [ ] Create GitHub repository
- [ ] Configure as template repository
- [ ] Set up branch protection
- [ ] Create directory structure
- [ ] Add `.gitignore`, `.editorconfig`, `LICENSE`
- [ ] Add VS Code settings

### Phase 2: Backend Foundation
- [ ] Create Azure Functions project
- [ ] Configure `Program.cs` with DI
- [ ] Set up structured logging
- [ ] Configure Application Insights (optional)
- [ ] Create `AppDbContext`
- [ ] Add sample entity + migration
- [ ] Create health endpoint
- [ ] Set up SignalR hub
- [ ] Create sample CRUD endpoints

### Phase 3: Frontend Foundation
- [ ] Initialize Vite + React + TypeScript
- [ ] Configure path aliases
- [ ] Copy COBRA theme from cobra-poc
- [ ] Copy styled components from cobra-poc
- [ ] Create AppLayout + navigation components
- [ ] Create ProfileMenu (minimal)
- [ ] Set up API service layer
- [ ] Create SignalR hook
- [ ] Create sample Notes page
- [ ] Configure `staticwebapp.config.json`

### Phase 4: CI/CD Pipeline
- [ ] Create `deploy.yml` workflow
- [ ] Create `test.yml` workflow
- [ ] Create `deploy-api-only.yml` workflow
- [ ] Add issue templates
- [ ] Add PR template
- [ ] Document required secrets

### Phase 5: Documentation
- [ ] Write `README.md`
- [ ] Write `CLAUDE.md`
- [ ] Write `CODING_STANDARDS.md`
- [ ] Write `GETTING_STARTED.md`
- [ ] Write `DEVELOPMENT_WORKFLOW.md`
- [ ] Write `LOGGING_GUIDE.md`
- [ ] Write `COBRA_STYLING.md`
- [ ] Write `NAVIGATION_CUSTOMIZATION.md`
- [ ] Write `SIGNALR_GUIDE.md`
- [ ] Write `DEPLOYMENT.md`
- [ ] Write architecture docs
- [ ] Create sample user stories

### Phase 6: Sample Feature
- [ ] Implement Notes backend
- [ ] Implement Notes frontend
- [ ] Add SignalR real-time updates
- [ ] Write tests

### Phase 7: Testing & Validation
- [ ] Write backend unit tests
- [ ] Write frontend component tests
- [ ] Deploy to test Azure environment
- [ ] Validate CI/CD pipeline
- [ ] Test all documented workflows
- [ ] Review and finalize documentation

---

## Estimated Effort

| Phase | Estimated Time |
|-------|---------------|
| Phase 1: Repository Setup | 2-3 hours |
| Phase 2: Backend Foundation | 8-12 hours |
| Phase 3: Frontend Foundation | 8-12 hours |
| Phase 4: CI/CD Pipeline | 4-6 hours |
| Phase 5: Documentation | 8-12 hours |
| Phase 6: Sample Feature | 4-6 hours |
| Phase 7: Testing & Validation | 4-8 hours |
| **Total** | **38-59 hours** |

---

## Next Steps

1. **Review this plan** - Confirm scope and priorities
2. **Create the repository** - Set up GitHub repo structure
3. **Begin Phase 2** - Backend foundation first (establishes API patterns)
4. **Iterate** - Build frontend once backend patterns are stable

---

## Questions/Decisions Needed

1. **Repository visibility:** Public or private?
2. **Azure Functions runtime:** .NET 8 (LTS) or .NET 9?
3. **MUI version:** 6.x (stable) or 7.x (latest)?
4. **Sample feature:** Notes, or prefer something else?
5. **Test Azure environment:** Need credentials/subscription access?
