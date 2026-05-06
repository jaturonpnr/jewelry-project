using JewelryFactory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryFactory.Infrastructure.Persistence.Configurations;

public class FinishedGoodsConfiguration : IEntityTypeConfiguration<FinishedGoods>
{
    public void Configure(EntityTypeBuilder<FinishedGoods> builder)
    {
        builder.ToTable("FinishedGoods");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SerialNumber).IsRequired().HasMaxLength(100);
        builder.Property(x => x.DesignCode).HasMaxLength(50);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(500);
        builder.Property(x => x.StoneSummary).HasMaxLength(2000);
        builder.Property(x => x.Location).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.Property(x => x.TotalGoldWeightGrams).HasPrecision(10, 4);
        builder.Property(x => x.TotalStoneCarat).HasPrecision(10, 4);
        builder.Property(x => x.TotalCost).HasPrecision(18, 2);
        builder.Property(x => x.ListPrice).HasPrecision(18, 2);

        builder.Property(x => x.Currency).HasConversion<string>().HasMaxLength(10);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasIndex(x => x.SerialNumber).IsUnique().HasDatabaseName("IX_FinishedGoods_SerialNumber");
        builder.HasIndex(x => x.Status).HasDatabaseName("IX_FinishedGoods_Status");
    }
}
