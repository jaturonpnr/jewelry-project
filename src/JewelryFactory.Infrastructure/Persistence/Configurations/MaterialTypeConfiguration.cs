using JewelryFactory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryFactory.Infrastructure.Persistence.Configurations;

public class MaterialTypeConfiguration : IEntityTypeConfiguration<MaterialType>
{
    public void Configure(EntityTypeBuilder<MaterialType> builder)
    {
        builder.ToTable("MaterialTypes");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Code).IsRequired().HasMaxLength(50);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(150);
        builder.Property(m => m.Description).HasMaxLength(500);

        builder.Property(m => m.Category).HasConversion<string>().HasMaxLength(30);
        builder.Property(m => m.Unit).HasConversion<string>().HasMaxLength(20);

        // Per CLAUDE.md §7: gold purity decimal(5,4)
        builder.Property(m => m.PurityFraction).HasPrecision(5, 4);

        builder.HasIndex(m => m.Code).IsUnique().HasDatabaseName("IX_MaterialTypes_Code");
        builder.HasIndex(m => new { m.Category, m.IsActive }).HasDatabaseName("IX_MaterialTypes_Category_IsActive");
    }
}
