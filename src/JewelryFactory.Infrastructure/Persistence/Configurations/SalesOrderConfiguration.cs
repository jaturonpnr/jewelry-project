using JewelryFactory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryFactory.Infrastructure.Persistence.Configurations;

public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("SalesOrders");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.CancellationReason).HasMaxLength(500);
        builder.Property(x => x.TrackingNumber).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Currency).HasConversion<string>().HasMaxLength(10);

        builder.Property(x => x.ExchangeRateToBase).HasPrecision(18, 6);
        builder.Property(x => x.Subtotal).HasPrecision(18, 2);
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 2);
        builder.Property(x => x.ShippingCost).HasPrecision(18, 2);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);

        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.OwnsOne(x => x.ShippingAddress, a =>
        {
            a.Property(p => p.Line1).HasColumnName("ShipAddressLine1").HasMaxLength(200);
            a.Property(p => p.Line2).HasColumnName("ShipAddressLine2").HasMaxLength(200);
            a.Property(p => p.City).HasColumnName("ShipCity").HasMaxLength(100);
            a.Property(p => p.State).HasColumnName("ShipState").HasMaxLength(100);
            a.Property(p => p.PostalCode).HasColumnName("ShipPostalCode").HasMaxLength(20);
            a.Property(p => p.Country).HasColumnName("ShipCountry").HasMaxLength(100);
        });

        builder.HasOne(x => x.Customer)
               .WithMany()
               .HasForeignKey(x => x.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
               .WithOne(i => i.SalesOrder)
               .HasForeignKey(i => i.SalesOrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.OrderNumber).IsUnique().HasDatabaseName("IX_SalesOrders_OrderNumber");
        builder.HasIndex(x => new { x.CustomerId, x.Status }).HasDatabaseName("IX_SalesOrders_Customer_Status");
        builder.HasIndex(x => x.OrderDate).HasDatabaseName("IX_SalesOrders_OrderDate");
    }
}

public class SalesOrderItemConfiguration : IEntityTypeConfiguration<SalesOrderItem>
{
    public void Configure(EntityTypeBuilder<SalesOrderItem> builder)
    {
        builder.ToTable("SalesOrderItems");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description).IsRequired().HasMaxLength(500);
        builder.Property(x => x.DesignCode).HasMaxLength(50);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.LineDiscount).HasPrecision(18, 2);
        builder.Property(x => x.LineTotal).HasPrecision(18, 2);

        builder.HasOne(x => x.FinishedGoods)
               .WithMany()
               .HasForeignKey(x => x.FinishedGoodsId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.SalesOrderId).HasDatabaseName("IX_SalesOrderItems_SalesOrderId");
    }
}
