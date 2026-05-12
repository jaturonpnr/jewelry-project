using JewelryFactory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryFactory.Infrastructure.Persistence.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.ShipmentNumber).IsRequired().HasMaxLength(50);
        builder.Property(s => s.Carrier).HasMaxLength(100);
        builder.Property(s => s.TrackingNumber).HasMaxLength(100);
        builder.Property(s => s.ShippingMethod).HasMaxLength(100);
        builder.Property(s => s.PackingNotes).HasMaxLength(2000);
        builder.Property(s => s.TotalWeightGrams).HasColumnType("decimal(10,4)");

        builder.HasIndex(s => s.ShipmentNumber).IsUnique();

        builder.HasOne(s => s.SalesOrder)
            .WithMany()
            .HasForeignKey(s => s.SalesOrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(s => s.Items)
            .WithOne(i => i.Shipment)
            .HasForeignKey(i => i.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("Shipments");
    }
}

public class ShipmentItemConfiguration : IEntityTypeConfiguration<ShipmentItem>
{
    public void Configure(EntityTypeBuilder<ShipmentItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Description).IsRequired().HasMaxLength(300);
        builder.Property(i => i.DesignCode).HasMaxLength(50);
        builder.Property(i => i.WeightGrams).HasColumnType("decimal(10,4)");
        builder.Property(i => i.UnitValueUsd).HasColumnType("decimal(18,2)");

        builder.HasOne(i => i.WorkOrder)
            .WithMany()
            .HasForeignKey(i => i.WorkOrderId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.ToTable("ShipmentItems");
    }
}

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(50);
        builder.Property(i => i.PaymentTerms).HasMaxLength(200);
        builder.Property(i => i.Notes).HasMaxLength(2000);
        builder.Property(i => i.PaymentReference).HasMaxLength(200);

        builder.Property(i => i.ExchangeRateToThb).HasColumnType("decimal(18,6)");
        builder.Property(i => i.VatPercent).HasColumnType("decimal(5,2)");
        builder.Property(i => i.Subtotal).HasColumnType("decimal(18,2)");
        builder.Property(i => i.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.VatAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.TotalAmountThb).HasColumnType("decimal(18,2)");

        builder.HasIndex(i => i.InvoiceNumber).IsUnique();
        builder.HasIndex(i => new { i.CustomerId, i.Status });
        builder.HasIndex(i => i.DueDate);

        builder.HasOne(i => i.Customer)
            .WithMany()
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(i => i.SalesOrder)
            .WithMany()
            .HasForeignKey(i => i.SalesOrderId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(i => i.Shipment)
            .WithMany()
            .HasForeignKey(i => i.ShipmentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(i => i.LineItems)
            .WithOne(l => l.Invoice)
            .HasForeignKey(l => l.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("Invoices");
    }
}

public class InvoiceLineItemConfiguration : IEntityTypeConfiguration<InvoiceLineItem>
{
    public void Configure(EntityTypeBuilder<InvoiceLineItem> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Description).IsRequired().HasMaxLength(300);
        builder.Property(l => l.DesignCode).HasMaxLength(50);
        builder.Property(l => l.UnitPrice).HasColumnType("decimal(18,4)");
        builder.Property(l => l.DiscountPercent).HasColumnType("decimal(5,2)");
        builder.Property(l => l.LineTotal).HasColumnType("decimal(18,2)");

        builder.ToTable("InvoiceLineItems");
    }
}
