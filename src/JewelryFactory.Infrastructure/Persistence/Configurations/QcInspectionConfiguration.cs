using JewelryFactory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryFactory.Infrastructure.Persistence.Configurations;

public class QcInspectionConfiguration : IEntityTypeConfiguration<QcInspection>
{
    public void Configure(EntityTypeBuilder<QcInspection> builder)
    {
        builder.HasKey(q => q.Id);
        builder.Property(q => q.InspectorName).IsRequired().HasMaxLength(200);
        builder.Property(q => q.Notes).HasMaxLength(2000);
        builder.Property(q => q.ActualWeightGrams).HasColumnType("decimal(10,4)");
        builder.Property(q => q.ExpectedWeightGrams).HasColumnType("decimal(10,4)");

        // NoAction to avoid SQL Server multiple cascade path error
        builder.HasOne(q => q.WorkOrder)
            .WithMany()
            .HasForeignKey(q => q.WorkOrderId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(q => q.WorkOrderStage)
            .WithMany()
            .HasForeignKey(q => q.WorkOrderStageId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(q => q.RawMaterialItem)
            .WithMany()
            .HasForeignKey(q => q.RawMaterialItemId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(q => q.Inspector)
            .WithMany()
            .HasForeignKey(q => q.InspectorId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(q => q.Defects)
            .WithOne(d => d.QcInspection)
            .HasForeignKey(d => d.QcInspectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(q => q.InspectionDate);
        builder.HasIndex(q => new { q.WorkOrderId, q.InspectionType });

        builder.ToTable("QcInspections");
    }
}

public class QcDefectConfiguration : IEntityTypeConfiguration<QcDefect>
{
    public void Configure(EntityTypeBuilder<QcDefect> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.DefectType).IsRequired().HasMaxLength(100);
        builder.Property(d => d.Description).HasMaxLength(500);

        builder.ToTable("QcDefects");
    }
}
