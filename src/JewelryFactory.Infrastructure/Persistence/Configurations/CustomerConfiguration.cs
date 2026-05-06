using JewelryFactory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryFactory.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code).IsRequired().HasMaxLength(50);
        builder.Property(c => c.CompanyName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.ContactPerson).HasMaxLength(150);
        builder.Property(c => c.Email).HasMaxLength(150);
        builder.Property(c => c.Phone).HasMaxLength(50);
        builder.Property(c => c.TaxId).HasMaxLength(50);
        builder.Property(c => c.Notes).HasMaxLength(2000);

        builder.Property(c => c.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(c => c.DefaultCurrency).HasConversion<string>().HasMaxLength(10);
        builder.Property(c => c.PaymentTerms).HasConversion<string>().HasMaxLength(30);

        builder.Property(c => c.CreditLimit).HasPrecision(18, 2);

        builder.OwnsOne(c => c.Address, a =>
        {
            a.Property(p => p.Line1).HasColumnName("AddressLine1").HasMaxLength(200);
            a.Property(p => p.Line2).HasColumnName("AddressLine2").HasMaxLength(200);
            a.Property(p => p.City).HasColumnName("City").HasMaxLength(100);
            a.Property(p => p.State).HasColumnName("State").HasMaxLength(100);
            a.Property(p => p.PostalCode).HasColumnName("PostalCode").HasMaxLength(20);
            a.Property(p => p.Country).HasColumnName("Country").HasMaxLength(100);
        });

        builder.HasIndex(c => c.Code).IsUnique().HasDatabaseName("IX_Customers_Code");
        builder.HasIndex(c => c.CompanyName).HasDatabaseName("IX_Customers_CompanyName");
        builder.HasIndex(c => new { c.Type, c.IsActive }).HasDatabaseName("IX_Customers_Type_IsActive");
    }
}
