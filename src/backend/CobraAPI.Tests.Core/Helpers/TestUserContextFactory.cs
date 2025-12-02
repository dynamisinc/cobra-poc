namespace CobraAPI.Tests.Core.Helpers;

/// <summary>
/// Factory for creating UserContext objects for testing.
///
/// USAGE:
/// var user = TestUserContextFactory.CreateTestUser();
/// var admin = TestUserContextFactory.CreateAdminUser();
///
/// Provides common user configurations for different test scenarios:
/// - Standard contributor user
/// - Admin user with full permissions
/// - Readonly user for view-only scenarios
/// - Manager user (non-admin with Manage role)
/// </summary>
public static class TestUserContextFactory
{
    /// <summary>
    /// Creates a standard test user context (Contributor role).
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
    /// Creates an admin user context with full permissions.
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
    /// Creates a readonly user context for view-only scenarios.
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
    /// Creates a manage role user context (non-admin but can manage).
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
