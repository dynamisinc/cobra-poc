// Global using directives for CobraAPI
// These provide common imports across all files in the project

// Core infrastructure
global using CobraAPI.Core.Data;
global using CobraAPI.Core.Models;
global using CobraAPI.Core.Models.Configuration;
global using CobraAPI.Core.Extensions;
global using CobraAPI.Core.Middleware;
global using CobraAPI.Core.Services;

// Shared modules
global using CobraAPI.Shared.Events.Models.Entities;
global using CobraAPI.Shared.Events.Models.DTOs;
global using CobraAPI.Shared.Events.Services;

// Admin module
global using CobraAPI.Admin.Models.Entities;
global using CobraAPI.Admin.Models;

// Checklist tool
global using CobraAPI.Tools.Checklist.Models.Entities;
global using CobraAPI.Tools.Checklist.Models.DTOs;
global using CobraAPI.Tools.Checklist.Models.Enums;
global using CobraAPI.Tools.Checklist.Services;
global using CobraAPI.Tools.Checklist.Hubs;
global using CobraAPI.Tools.Checklist.Mappers;

// Chat tool
global using CobraAPI.Tools.Chat.Models.Entities;
global using CobraAPI.Tools.Chat.Models.DTOs;
global using CobraAPI.Tools.Chat.Services;
global using CobraAPI.Tools.Chat.Hubs;
global using CobraAPI.Tools.Chat.ExternalPlatforms;

// Analytics tool
global using CobraAPI.Tools.Analytics.Models.DTOs;
global using CobraAPI.Tools.Analytics.Services;
