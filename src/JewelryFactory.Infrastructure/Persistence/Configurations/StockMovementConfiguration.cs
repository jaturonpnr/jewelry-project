using JewelryFactory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryFactory.Infrastructure.Persistence.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ItemType).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.MovementType).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.QuantityDelta).HasPrecision(18, 4);
        builder.Property(x => x.QuantityAfter).HasPrecision(18, 4);

        builder.Property(x => x.ReferenceType).HasMaxLength(50);
        builder.Property(x => x.Reason).HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.PerformedBy).IsRequired().HasMaxLength(100);

        builder.HasIndex(x => new { x.ItemType, x.ItemId }).HasDatabaseName("IX_StockMovements_Item");
        builder.HasIndex(x => x.PerformedAt).HasDatabaseName("IX_StockMovements_PerformedAt");
    }
}
