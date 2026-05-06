using JewelryFactory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryFactory.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).IsRequired().HasMaxLength(150);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
        builder.Property(u => u.FullName).IsRequired().HasMaxLength(150);
        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(30);
        builder.Property(u => u.CreatedBy).HasMaxLength(100);
        builder.Property(u => u.UpdatedBy).HasMaxLength(100);

        // Unique index on Email (case-insensitive via collation handled by SQL Server default)
        builder.HasIndex(u => u.Email).IsUnique().HasDatabaseName("IX_Users_Email");
        builder.HasIndex(u => new { u.Role, u.IsActive }).HasDatabaseName("IX_Users_Role_IsActive");

        builder.HasMany(u => u.RefreshTokens)
               .WithOne(rt => rt.User)
               .HasForeignKey(rt => rt.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
