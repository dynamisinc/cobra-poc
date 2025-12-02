using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CobraAPI.Tests.Core.Helpers;

/// <summary>
/// Factory for creating in-memory database contexts for testing.
///
/// USAGE:
/// var context = TestDbContextFactory.CreateInMemoryContext();
///
/// Each call creates a unique database instance for test isolation.
/// The in-memory database is automatically disposed with the context.
/// </summary>
public static class TestDbContextFactory
{
    /// <summary>
    /// Creates a new CobraDbContext with an in-memory database.
    /// Each call creates a unique database instance for test isolation.
    /// </summary>
    /// <returns>A new CobraDbContext connected to a unique in-memory database</returns>
    public static CobraDbContext CreateInMemoryContext()
    {
        // Create a service provider with in-memory database configured
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider();

        var options = new DbContextOptionsBuilder<CobraDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .UseInternalServiceProvider(serviceProvider)
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new CobraDbContext(options);
    }
}
