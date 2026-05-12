using JewelryFactory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace JewelryFactory.Application.Common.Interfaces;

/// <summary>
/// DbContext abstraction so Application layer doesn't depend on EF Core specifics.
/// Infrastructure provides the concrete implementation.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    // Phase 1 - Module 2 (Master Data)
    DbSet<Customer> Customers { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<Worker> Workers { get; }
    DbSet<MaterialType> MaterialTypes { get; }

    // Phase 1 - Module 3 (Inventory)
    DbSet<RawMaterialItem> RawMaterialItems { get; }
    DbSet<StoneItem> StoneItems { get; }
    DbSet<StoneParcel> StoneParcels { get; }
    DbSet<FinishedGoods> FinishedGoods { get; }
    DbSet<StockMovement> StockMovements { get; }

    // Phase 1 - Module 4 (Sales Order)
    DbSet<SalesOrder> SalesOrders { get; }
    DbSet<SalesOrderItem> SalesOrderItems { get; }

    // Phase 1 - Module 5 (Production Tracking)
    DbSet<WorkOrder> WorkOrders { get; }
    DbSet<WorkOrderStage> WorkOrderStages { get; }

    // Phase 2 - Module 7 (BOM & Costing)
    DbSet<BomTemplate> BomTemplates { get; }
    DbSet<BomMaterialLine> BomMaterialLines { get; }
    DbSet<BomStoneLine> BomStoneLines { get; }
    DbSet<BomLaborLine> BomLaborLines { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Access entity entry — needed for setting OriginalValue of RowVersion (optimistic concurrency).
    /// </summary>
    EntityEntry<T> Entry<T>(T entity) where T : class;
}
