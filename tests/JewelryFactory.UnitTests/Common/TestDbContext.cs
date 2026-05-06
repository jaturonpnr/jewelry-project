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
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Worker> Workers => Set<Worker>();
    public DbSet<MaterialType> MaterialTypes => Set<MaterialType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Address is a value object — declare as owned so InMemory provider doesn't need a PK.
        modelBuilder.Entity<Customer>().OwnsOne(c => c.Address);
        modelBuilder.Entity<Supplier>().OwnsOne(s => s.Address);
        base.OnModelCreating(modelBuilder);
    }
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
