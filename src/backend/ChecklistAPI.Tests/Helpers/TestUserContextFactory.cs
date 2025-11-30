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
        bool isAdmin = false,
        PermissionRole role = PermissionRole.Contributor)
    {
        return new UserContext
        {
            Email = email,
            FullName = fullName,
            Position = position,
            IsAdmin = isAdmin,
            Role = role
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
            IsAdmin = true,
            Role = PermissionRole.Manage
        };
    }

    /// <summary>
    /// Creates a readonly user context
    /// </summary>
    public static UserContext CreateReadonlyUser(
        string email = "readonly@example.com",
        string fullName = "Readonly User",
        string position = "Observer")
    {
        return new UserContext
        {
            Email = email,
            FullName = fullName,
            Position = position,
            IsAdmin = false,
            Role = PermissionRole.Readonly
        };
    }

    /// <summary>
    /// Creates a manage role user context (non-admin but can manage)
    /// </summary>
    public static UserContext CreateManagerUser(
        string email = "manager@example.com",
        string fullName = "Manager User",
        string position = "Planning Section Chief")
    {
        return new UserContext
        {
            Email = email,
            FullName = fullName,
            Position = position,
            IsAdmin = false,
            Role = PermissionRole.Manage
        };
    }
}