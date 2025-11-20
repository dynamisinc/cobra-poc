using ChecklistAPI.Models;

namespace ChecklistAPI.Tests.Helpers;

/// <summary>
/// Factory for creating UserContext objects for testing
/// </summary>
public static class TestUserContextFactory
{
    /// <summary>
    /// Creates a standard test user context
    /// </summary>
    public static UserContext CreateTestUser(
        string email = "test@example.com",
        string fullName = "Test User",
        string position = "Safety Officer",
        bool isAdmin = false)
    {
        return new UserContext
        {
            Email = email,
            FullName = fullName,
            Position = position,
            IsAdmin = isAdmin
        };
    }

    /// <summary>
    /// Creates an admin user context
    /// </summary>
    public static UserContext CreateAdminUser()
    {
        return new UserContext
        {
            Email = "admin@example.com",
            FullName = "Admin User",
            Position = "Incident Commander",
            IsAdmin = true
        };
    }
}