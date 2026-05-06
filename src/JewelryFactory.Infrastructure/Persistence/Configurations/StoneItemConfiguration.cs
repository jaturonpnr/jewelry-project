using JewelryFactory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryFactory.Infrastructure.Persistence.Configurations;

public class StoneItemConfiguration : IEntityTypeConfiguration<StoneItem>
{
    public void Configure(EntityTypeBuilder<StoneItem> builder)
    {
        builder.ToTable("StoneItems");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ItemCode).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Cut).HasMaxLength(50);
        builder.Property(x => x.Measurements).HasMaxLength(100);
        builder.Property(x => x.CertificateNumber).HasMaxLength(100);
        builder.Property(x => x.Origin).HasMaxLength(100);
        builder.Property(x => x.Location).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        // CLAUDE.md §7: carat decimal(8,4)
        builder.Property(x => x.CaratWeight).HasPrecision(8, 4);
        builder.Property(x => x.UnitCost).HasPrecision(18, 2);

        builder.Property(x => x.Shape).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Color).HasConversion<string>().HasMaxLength(10);
        builder.Property(x => x.Clarity).HasConversion<string>().HasMaxLength(10);
        builder.Property(x => x.CertAuthority).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.CostCurrency).HasConversion<string>().HasMaxLength(10);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasOne(x => x.MaterialType)
               .WithMany().HasForeignKey(x => x.MaterialTypeId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Supplier)
               .WithMany().HasForeignKey(x => x.SupplierId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.ItemCode).IsUnique().HasDatabaseName("IX_StoneItems_ItemCode");
        builder.HasIndex(x => x.CertificateNumber).HasDatabaseName("IX_StoneItems_CertificateNumber");
        builder.HasIndex(x => x.Status).HasDatabaseName("IX_StoneItems_Status");
    }
}
