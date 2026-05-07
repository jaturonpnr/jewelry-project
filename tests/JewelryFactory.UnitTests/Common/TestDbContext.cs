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

    public DbSet<RawMaterialItem> RawMaterialItems => Set<RawMaterialItem>();
    public DbSet<StoneItem> StoneItems => Set<StoneItem>();
    public DbSet<StoneParcel> StoneParcels => Set<StoneParcel>();
    public DbSet<FinishedGoods> FinishedGoods => Set<FinishedGoods>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderItem> SalesOrderItems => Set<SalesOrderItem>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<WorkOrderStage> WorkOrderStages => Set<WorkOrderStage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Address is a value object — declare as owned so InMemory provider doesn't need a PK.
        modelBuilder.Entity<Customer>().OwnsOne(c => c.Address);
        modelBuilder.Entity<Supplier>().OwnsOne(s => s.Address);
        modelBuilder.Entity<SalesOrder>().OwnsOne(o => o.ShippingAddress);
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
