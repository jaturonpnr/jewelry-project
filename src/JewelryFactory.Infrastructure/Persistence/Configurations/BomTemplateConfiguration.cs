using JewelryFactory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryFactory.Infrastructure.Persistence.Configurations;

public class BomTemplateConfiguration : IEntityTypeConfiguration<BomTemplate>
{
    public void Configure(EntityTypeBuilder<BomTemplate> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.DesignCode).IsRequired().HasMaxLength(50);
        builder.Property(b => b.DesignName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Description).HasMaxLength(1000);
        builder.Property(b => b.OverheadPercent).HasColumnType("decimal(5,2)");

        builder.HasIndex(b => b.DesignCode).IsUnique().HasFilter("[IsDeleted] = 0");

        builder.HasMany(b => b.MaterialLines)
            .WithOne(m => m.BomTemplate)
            .HasForeignKey(m => m.BomTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.StoneLines)
            .WithOne(s => s.BomTemplate)
            .HasForeignKey(s => s.BomTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.LaborLines)
            .WithOne(l => l.BomTemplate)
            .HasForeignKey(l => l.BomTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("BomTemplates");
    }
}

public class BomMaterialLineConfiguration : IEntityTypeConfiguration<BomMaterialLine>
{
    public void Configure(EntityTypeBuilder<BomMaterialLine> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.MaterialDescription).IsRequired().HasMaxLength(200);
        builder.Property(m => m.QuantityGrams).HasColumnType("decimal(10,4)");
        builder.Property(m => m.ExpectedLossPercent).HasColumnType("decimal(5,2)");
        builder.Property(m => m.UnitCostThbPerGram).HasColumnType("decimal(18,4)");
        builder.Property(m => m.PurityFraction).HasColumnType("decimal(5,4)");

        builder.ToTable("BomMaterialLines");
    }
}

public class BomStoneLineConfiguration : IEntityTypeConfiguration<BomStoneLine>
{
    public void Configure(EntityTypeBuilder<BomStoneLine> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.StoneType).IsRequired().HasMaxLength(100);
        builder.Property(s => s.StoneShape).IsRequired().HasMaxLength(100);
        builder.Property(s => s.SizeDescription).IsRequired().HasMaxLength(200);
        builder.Property(s => s.CaratPerStone).HasColumnType("decimal(8,4)");
        builder.Property(s => s.UnitCostThbPerCarat).HasColumnType("decimal(18,4)");

        // Computed properties — not stored in DB
        builder.Ignore(s => s.TotalCaratWeight);
        builder.Ignore(s => s.LineCostThb);

        builder.ToTable("BomStoneLines");
    }
}

public class BomLaborLineConfiguration : IEntityTypeConfiguration<BomLaborLine>
{
    public void Configure(EntityTypeBuilder<BomLaborLine> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.EstimatedHours).HasColumnType("decimal(8,2)");
        builder.Property(l => l.HourlyRateThb).HasColumnType("decimal(18,2)");

        builder.Ignore(l => l.LaborCostThb);

        builder.ToTable("BomLaborLines");
    }
}
