using System.Reflection;
using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Domain.Common;
using JewelryFactory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Infrastructure.Persistence;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ICurrentUserService currentUser
) : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Phase 1 - Module 2 (Master Data)
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Worker> Workers => Set<Worker>();
    public DbSet<MaterialType> MaterialTypes => Set<MaterialType>();

    // Phase 1 - Module 3 (Inventory)
    public DbSet<RawMaterialItem> RawMaterialItems => Set<RawMaterialItem>();
    public DbSet<StoneItem> StoneItems => Set<StoneItem>();
    public DbSet<StoneParcel> StoneParcels => Set<StoneParcel>();
    public DbSet<FinishedGoods> FinishedGoods => Set<FinishedGoods>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    // Phase 1 - Module 4 (Sales Order)
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderItem> SalesOrderItems => Set<SalesOrderItem>();

    // Phase 1 - Module 5 (Production Tracking)
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<WorkOrderStage> WorkOrderStages => Set<WorkOrderStage>();

    // Phase 2 - Module 7 (BOM & Costing)
    public DbSet<BomTemplate> BomTemplates => Set<BomTemplate>();
    public DbSet<BomMaterialLine> BomMaterialLines => Set<BomMaterialLine>();
    public DbSet<BomStoneLine> BomStoneLines => Set<BomStoneLine>();
    public DbSet<BomLaborLine> BomLaborLines => Set<BomLaborLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all IEntityTypeConfiguration<> in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Global soft-delete filter for AuditableEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var prop = System.Linq.Expressions.Expression.Property(parameter, nameof(AuditableEntity.IsDeleted));
                var notDeleted = System.Linq.Expressions.Expression.Equal(prop,
                    System.Linq.Expressions.Expression.Constant(false));
                var lambda = System.Linq.Expressions.Expression.Lambda(notDeleted, parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var userId = currentUser.UserId?.ToString() ?? "system";

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = userId;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = userId;
                    break;
                case EntityState.Deleted:
                    // Convert hard delete → soft delete (per CLAUDE.md §10.10)
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = userId;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
