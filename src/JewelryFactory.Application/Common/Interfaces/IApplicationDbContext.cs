using JewelryFactory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Common.Interfaces;

/// <summary>
/// DbContext abstraction so Application layer doesn't depend on EF Core specifics.
/// Infrastructure provides the concrete implementation.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
