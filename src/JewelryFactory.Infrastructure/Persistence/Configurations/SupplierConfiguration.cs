using JewelryFactory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryFactory.Infrastructure.Persistence.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Code).IsRequired().HasMaxLength(50);
        builder.Property(s => s.CompanyName).IsRequired().HasMaxLength(200);
        builder.Property(s => s.ContactPerson).HasMaxLength(150);
        builder.Property(s => s.Email).HasMaxLength(150);
        builder.Property(s => s.Phone).HasMaxLength(50);
        builder.Property(s => s.TaxId).HasMaxLength(50);
        builder.Property(s => s.Certifications).HasMaxLength(500);
        builder.Property(s => s.Notes).HasMaxLength(2000);

        builder.Property(s => s.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(s => s.DefaultCurrency).HasConversion<string>().HasMaxLength(10);
        builder.Property(s => s.PaymentTerms).HasConversion<string>().HasMaxLength(30);

        builder.OwnsOne(s => s.Address, a =>
        {
            a.Property(p => p.Line1).HasColumnName("AddressLine1").HasMaxLength(200);
            a.Property(p => p.Line2).HasColumnName("AddressLine2").HasMaxLength(200);
            a.Property(p => p.City).HasColumnName("City").HasMaxLength(100);
            a.Property(p => p.State).HasColumnName("State").HasMaxLength(100);
            a.Property(p => p.PostalCode).HasColumnName("PostalCode").HasMaxLength(20);
            a.Property(p => p.Country).HasColumnName("Country").HasMaxLength(100);
        });

        builder.HasIndex(s => s.Code).IsUnique().HasDatabaseName("IX_Suppliers_Code");
        builder.HasIndex(s => new { s.Type, s.IsActive }).HasDatabaseName("IX_Suppliers_Type_IsActive");
    }
}
