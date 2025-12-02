// =============================================================================
// GlobalUsings.cs - C# 10+ Global Using Directives (Chat Test Project)
// =============================================================================
//
// WHAT IS THIS?
// Global using directives for the Chat tool test project.
// These imports are available in all test files within this project.
//
// LIFT-AND-SHIFT:
// When moving the Chat tool to production, this file defines all
// the namespaces needed for tests. Update paths as needed after migration.
//
// SEE ALSO:
// - CobraAPI.Tests.Core/GlobalUsings.cs - Shared test infrastructure
// - CobraAPI/Tools/Chat/ - The code being tested
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

// Chat tool (the code being tested)
global using CobraAPI.Tools.Chat.Models.Entities;
global using CobraAPI.Tools.Chat.Models.DTOs;
global using CobraAPI.Tools.Chat.Services;
global using CobraAPI.Tools.Chat.Hubs;
global using CobraAPI.Tools.Chat.ExternalPlatforms;

// Test helpers from Core test project
global using CobraAPI.Tests.Core.Helpers;
