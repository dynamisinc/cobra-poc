// =============================================================================
// GlobalUsings.cs - C# 10+ Global Using Directives (Checklist Test Project)
// =============================================================================
//
// WHAT IS THIS?
// Global using directives for the Checklist tool test project.
// These imports are available in all test files within this project.
//
// LIFT-AND-SHIFT:
// When moving the Checklist tool to production, this file defines all
// the namespaces needed for tests. Update paths as needed after migration.
//
// SEE ALSO:
// - CobraAPI.Tests.Core/GlobalUsings.cs - Shared test infrastructure
// - CobraAPI/Tools/Checklist/ - The code being tested
//
// =============================================================================

// Core infrastructure
global using CobraAPI.Core.Data;
global using CobraAPI.Core.Models;
global using CobraAPI.Core.Models.Configuration;

// Shared modules
global using CobraAPI.Shared.Events.Models.Entities;
global using CobraAPI.Shared.Events.Models.DTOs;
global using CobraAPI.Shared.Events.Services;

// Checklist tool (the code being tested)
global using CobraAPI.Tools.Checklist.Models.Entities;
global using CobraAPI.Tools.Checklist.Models.DTOs;
global using CobraAPI.Tools.Checklist.Models.Enums;
global using CobraAPI.Tools.Checklist.Services;
global using CobraAPI.Tools.Checklist.Controllers;
global using CobraAPI.Tools.Checklist.Hubs;

// Analytics tool (tests aggregation of checklist data)
global using CobraAPI.Tools.Analytics.Services;

// Test helpers from Core test project
global using CobraAPI.Tests.Core.Helpers;
