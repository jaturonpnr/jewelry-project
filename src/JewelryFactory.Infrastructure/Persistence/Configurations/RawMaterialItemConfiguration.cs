using JewelryFactory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryFactory.Infrastructure.Persistence.Configurations;

public class RawMaterialItemConfiguration : IEntityTypeConfiguration<RawMaterialItem>
{
    public void Configure(EntityTypeBuilder<RawMaterialItem> builder)
    {
        builder.ToTable("RawMaterialItems");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LotNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Location).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        // Per CLAUDE.md §7
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.PureWeight).HasPrecision(18, 4);
        builder.Property(x => x.UnitCost).HasPrecision(18, 2);

        builder.Property(x => x.CostCurrency).HasConversion<string>().HasMaxLength(10);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        // Optimistic concurrency token (CLAUDE.md §12.7)
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasOne(x => x.MaterialType)
               .WithMany()
               .HasForeignKey(x => x.MaterialTypeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Supplier)
               .WithMany()
               .HasForeignKey(x => x.SupplierId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.MaterialTypeId, x.LotNumber, x.Location })
               .HasDatabaseName("IX_RawMaterialItems_MaterialType_Lot_Location");
        builder.HasIndex(x => x.Status).HasDatabaseName("IX_RawMaterialItems_Status");
    }
}
