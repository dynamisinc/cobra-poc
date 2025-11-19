using ChecklistAPI.Models;

namespace ChecklistAPI.Middleware;

/// <summary>
/// MockUserMiddleware - Simulates authentication for POC development
///
/// Purpose:
///   Provides a mock authenticated user context without requiring
///   real authentication infrastructure (Azure AD, OAuth, etc.).
///   This is ONLY for POC demonstration purposes.
///
/// How It Works:
///   1. Reads mock user settings from appsettings.json (MockAuth section)
///   2. Creates a UserContext with configured user details
///   3. Injects UserContext into HttpContext.Items for controller access
///   4. Logs user context creation for debugging
///
/// Production Replacement:
///   In production, replace this with real authentication middleware that:
///   - Validates JWT tokens or OAuth claims
///   - Extracts user info from claims
///   - Integrates with Azure AD or similar
///
/// Configuration (appsettings.json):
///   {
///     "MockAuth": {
///       "Enabled": true,
///       "DefaultUser": "admin@cobra.mil",
///       "DefaultPosition": "Incident Commander"
///     }
///   }
///
/// Usage in Controllers:
///   var userContext = HttpContext.Items["UserContext"] as UserContext;
///
/// Author: Checklist POC Team
/// Last Modified: 2025-11-19
/// </summary>
public class MockUserMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MockUserMiddleware> _logger;
    private readonly IConfiguration _configuration;

    public MockUserMiddleware(
        RequestDelegate next,
        ILogger<MockUserMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Processes the HTTP request and injects mock user context
    /// </summary>
    /// <param name="context">Current HTTP context</param>
    public async Task InvokeAsync(HttpContext context)
    {
        // Check if mock auth is enabled
        var mockAuthEnabled = _configuration.GetValue<bool>("MockAuth:Enabled", true);

        if (mockAuthEnabled)
        {
            // Read mock user settings from configuration
            var email = _configuration["MockAuth:DefaultUser"] ?? "admin@cobra.mil";
            var position = _configuration["MockAuth:DefaultPosition"] ?? "Incident Commander";

            // Create mock user context
            var userContext = new UserContext
            {
                Email = email,
                FullName = ExtractNameFromEmail(email),
                Position = position,
                IsAdmin = true, // POC: All mock users are admins
                CurrentEventId = null, // Could be set from query string in future
                CurrentOperationalPeriod = null
            };

            // Inject into HttpContext for controller access
            context.Items["UserContext"] = userContext;

            // Log for debugging (only on first request to avoid spam)
            _logger.LogInformation(
                "Mock user context created: {Email} ({Position})",
                userContext.Email,
                userContext.Position
            );
        }
        else
        {
            _logger.LogWarning("MockAuth is disabled but no real auth is configured!");
        }

        // Continue to next middleware
        await _next(context);
    }

    /// <summary>
    /// Extracts a display name from an email address
    /// Example: "john.smith@cobra.mil" -> "John Smith"
    /// </summary>
    private string ExtractNameFromEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return "Unknown User";

        // Get the local part before @
        var localPart = email.Split('@')[0];

        // Replace dots/underscores with spaces and title case
        var name = localPart.Replace('.', ' ').Replace('_', ' ');
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);
    }
}
