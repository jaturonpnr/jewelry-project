using JewelryFactory.Application.Common.Interfaces;

namespace JewelryFactory.Infrastructure.Services;

/// <summary>
/// BCrypt password hasher with work factor 12 (per CLAUDE.md §9).
/// </summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);
}
