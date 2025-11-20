using ChecklistAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace ChecklistAPI.Tests.Helpers;

/// <summary>
/// Factory for creating in-memory database contexts for testing
/// </summary>
public static class TestDbContextFactory
{
    /// <summary>
    /// Creates a new ChecklistDbContext with an in-memory database
    /// Each call creates a unique database instance for test isolation
    /// </summary>
    public static ChecklistDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ChecklistDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ChecklistDbContext(options);
    }
}