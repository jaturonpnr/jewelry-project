using JewelryFactory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryFactory.Infrastructure.Persistence.Configurations;

public class WorkerConfiguration : IEntityTypeConfiguration<Worker>
{
    public void Configure(EntityTypeBuilder<Worker> builder)
    {
        builder.ToTable("Workers");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.EmployeeCode).IsRequired().HasMaxLength(50);
        builder.Property(w => w.FullName).IsRequired().HasMaxLength(150);
        builder.Property(w => w.Phone).HasMaxLength(50);
        builder.Property(w => w.Email).HasMaxLength(150);
        builder.Property(w => w.Department).HasMaxLength(100);
        builder.Property(w => w.Skills).HasMaxLength(2000);
        builder.Property(w => w.Notes).HasMaxLength(2000);

        builder.Property(w => w.Position).HasConversion<string>().HasMaxLength(30);
        builder.Property(w => w.WageType).HasConversion<string>().HasMaxLength(20);
        builder.Property(w => w.WageCurrency).HasConversion<string>().HasMaxLength(10);
        builder.Property(w => w.WageRate).HasPrecision(18, 4);

        builder.HasIndex(w => w.EmployeeCode).IsUnique().HasDatabaseName("IX_Workers_EmployeeCode");
        builder.HasIndex(w => new { w.Position, w.IsActive }).HasDatabaseName("IX_Workers_Position_IsActive");

        // Optional 1-to-0..1 to User
        builder.HasOne(w => w.User)
               .WithMany()
               .HasForeignKey(w => w.UserId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
