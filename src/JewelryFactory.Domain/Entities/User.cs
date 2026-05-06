using JewelryFactory.Domain.Common;
using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Domain.Entities;

/// <summary>
/// System user. Authentication via email + password (BCrypt hashed).
/// Phase 1 — Module 1 (User & Auth).
/// </summary>
public class User : AuditableEntity
{
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string FullName { get; set; }
    public UserRole Role { get; set; } = UserRole.Worker;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    // Refresh tokens (one-to-many)
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
