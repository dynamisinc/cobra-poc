using CobraAPI.Core.Middleware;

namespace CobraAPI.Core.Extensions;

/// <summary>
/// MiddlewareExtensions - Clean extension methods for middleware registration
///
/// Purpose:
///   Provides fluent, readable extension methods to register custom middleware
///   in Program.cs. Keeps Program.cs clean and focused.
///
/// Pattern:
///   Instead of: app.UseMiddleware<MockUserMiddleware>();
///   Use: app.UseMockUserContext();
///
/// Benefits:
///   - Self-documenting code
///   - Consistent naming (Use* pattern)
///   - Easy to discover via IntelliSense
///   - Cleaner Program.cs
///
/// Author: Checklist POC Team
/// Last Modified: 2025-11-19
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Adds mock user authentication middleware to the pipeline
    /// This must be called BEFORE UseAuthorization() in Program.cs
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <returns>Application builder for chaining</returns>
    public static IApplicationBuilder UseMockUserContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<MockUserMiddleware>();
    }
}
