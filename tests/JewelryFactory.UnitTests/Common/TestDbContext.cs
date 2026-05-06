using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.UnitTests.Common;

/// <summary>
/// Lightweight in-memory IApplicationDbContext for unit tests.
/// Avoids depending on Infrastructure's ApplicationDbContext (which requires ICurrentUserService).
/// </summary>
public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
}

public static class TestDbContextFactory
{
    public static TestDbContext Create()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase($"JewelryTest_{Guid.NewGuid()}")
            .Options;
        return new TestDbContext(options);
    }
}
