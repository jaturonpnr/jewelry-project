using JewelryFactory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryFactory.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.TokenHash).IsRequired().HasMaxLength(255);
        builder.Property(rt => rt.ReplacedByTokenHash).HasMaxLength(255);

        builder.HasIndex(rt => rt.TokenHash).IsUnique().HasDatabaseName("IX_RefreshTokens_TokenHash");
        builder.HasIndex(rt => rt.UserId).HasDatabaseName("IX_RefreshTokens_UserId");

        // Match the User soft-delete filter so refresh tokens of deleted users are also hidden.
        builder.HasQueryFilter(rt => !rt.User.IsDeleted);
    }
}
