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

    // Phase 1 - Module 2 (Master Data)
    DbSet<Customer> Customers { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<Worker> Workers { get; }
    DbSet<MaterialType> MaterialTypes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
