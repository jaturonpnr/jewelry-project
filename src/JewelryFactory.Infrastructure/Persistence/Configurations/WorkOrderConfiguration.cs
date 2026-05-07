using JewelryFactory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryFactory.Infrastructure.Persistence.Configurations;

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.ToTable("WorkOrders");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.WorkOrderNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.DesignCode).HasMaxLength(50);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(500);
        builder.Property(x => x.CancellationReason).HasMaxLength(500);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Priority).HasConversion<string>().HasMaxLength(20);

        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasOne(x => x.SalesOrder)
               .WithMany()
               .HasForeignKey(x => x.SalesOrderId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.AssignedSupervisor)
               .WithMany()
               .HasForeignKey(x => x.AssignedSupervisorId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Stages)
               .WithOne(s => s.WorkOrder)
               .HasForeignKey(s => s.WorkOrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.WorkOrderNumber).IsUnique().HasDatabaseName("IX_WorkOrders_WorkOrderNumber");
        builder.HasIndex(x => new { x.Status, x.Priority }).HasDatabaseName("IX_WorkOrders_Status_Priority");
        builder.HasIndex(x => x.SalesOrderId).HasDatabaseName("IX_WorkOrders_SalesOrderId");
    }
}

public class WorkOrderStageConfiguration : IEntityTypeConfiguration<WorkOrderStage>
{
    public void Configure(EntityTypeBuilder<WorkOrderStage> builder)
    {
        builder.ToTable("WorkOrderStages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Stage).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        builder.Property(x => x.EstimatedHours).HasPrecision(10, 2);
        builder.Property(x => x.ActualHours).HasPrecision(10, 2);

        // Per CLAUDE.md §7: gold weight grams = decimal(10, 4)
        builder.Property(x => x.WeightInGrams).HasPrecision(10, 4);
        builder.Property(x => x.WeightOutGrams).HasPrecision(10, 4);

        // LossGrams is a computed read-only property — don't map it
        builder.Ignore(x => x.LossGrams);

        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.FailureReason).HasMaxLength(500);

        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasOne(x => x.AssignedWorker)
               .WithMany()
               .HasForeignKey(x => x.AssignedWorkerId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.WorkOrderId, x.SequenceNumber })
               .IsUnique()
               .HasDatabaseName("IX_WorkOrderStages_WorkOrder_Sequence");
        builder.HasIndex(x => x.Status).HasDatabaseName("IX_WorkOrderStages_Status");
    }
}
