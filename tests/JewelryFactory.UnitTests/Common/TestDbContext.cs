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

    // Phase 2 - Module 7 (BOM & Costing)
    public DbSet<BomTemplate> BomTemplates => Set<BomTemplate>();
    public DbSet<BomMaterialLine> BomMaterialLines => Set<BomMaterialLine>();
    public DbSet<BomStoneLine> BomStoneLines => Set<BomStoneLine>();
    public DbSet<BomLaborLine> BomLaborLines => Set<BomLaborLine>();

    // Phase 2 - Module 8 (Quality Control)
    public DbSet<QcInspection> QcInspections => Set<QcInspection>();
    public DbSet<QcDefect> QcDefects => Set<QcDefect>();

    // Phase 2 - Module 11 (Shipping & Invoice)
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentItem> ShipmentItems => Set<ShipmentItem>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLineItem> InvoiceLineItems => Set<InvoiceLineItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>().OwnsOne(c => c.Address);
        modelBuilder.Entity<Supplier>().OwnsOne(s => s.Address);
        modelBuilder.Entity<SalesOrder>().OwnsOne(o => o.ShippingAddress);
        // BomStoneLine computed properties — not stored
        modelBuilder.Entity<BomStoneLine>().Ignore(s => s.TotalCaratWeight).Ignore(s => s.LineCostThb);
        modelBuilder.Entity<BomLaborLine>().Ignore(l => l.LaborCostThb);
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
