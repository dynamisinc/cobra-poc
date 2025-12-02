// =============================================================================
// GlobalUsings.cs - C# 10+ Global Using Directives (Core Test Project)
// =============================================================================
//
// WHAT IS THIS?
// Global using directives for the shared test infrastructure project.
// This project contains test utilities used by all tool-specific test projects.
//
// WHY SEPARATE?
// This project is referenced by CobraAPI.Tests.Checklist, CobraAPI.Tests.Chat,
// etc. Each tool test project can be lifted and shifted independently while
// still referencing shared test infrastructure.
//
// SEE ALSO:
// - docs/BACKEND_ARCHITECTURE.md - Project organization
// - CobraAPI/GlobalUsings.cs - Main project global usings
//
// =============================================================================

// Core infrastructure (needed for test helpers)
global using CobraAPI.Core.Data;
global using CobraAPI.Core.Models;
global using CobraAPI.Core.Models.Configuration;

// Shared modules (Events are used across all tools)
global using CobraAPI.Shared.Events.Models.Entities;
global using CobraAPI.Shared.Events.Models.DTOs;
global using CobraAPI.Shared.Events.Services;
