using JewelryFactory.Domain.Common;

namespace JewelryFactory.Domain.Entities;

/// <summary>
/// Refresh token (hashed) for JWT rotation.
/// Per CLAUDE.md §9 — refresh tokens stored hashed, 7-day TTL.
/// </summary>
public class RefreshToken : BaseEntity
{
    public required string TokenHash { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;
}
