using JewelryFactory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryFactory.Infrastructure.Persistence.Configurations;

public class StoneParcelConfiguration : IEntityTypeConfiguration<StoneParcel>
{
    public void Configure(EntityTypeBuilder<StoneParcel> builder)
    {
        builder.ToTable("StoneParcels");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ParcelCode).IsRequired().HasMaxLength(50);
        builder.Property(x => x.QualityGrade).HasMaxLength(50);
        builder.Property(x => x.Location).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.Property(x => x.TotalCarat).HasPrecision(10, 4);
        builder.Property(x => x.AverageSize).HasPrecision(8, 4);
        builder.Property(x => x.UnitCostPerCarat).HasPrecision(18, 2);

        builder.Property(x => x.Shape).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.CostCurrency).HasConversion<string>().HasMaxLength(10);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasOne(x => x.MaterialType)
               .WithMany().HasForeignKey(x => x.MaterialTypeId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Supplier)
               .WithMany().HasForeignKey(x => x.SupplierId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.ParcelCode).IsUnique().HasDatabaseName("IX_StoneParcels_ParcelCode");
        builder.HasIndex(x => x.Status).HasDatabaseName("IX_StoneParcels_Status");
    }
}
