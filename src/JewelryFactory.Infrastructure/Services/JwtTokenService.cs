using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JewelryFactory.Infrastructure.Services;

/// <summary>
/// JWT generation + refresh token rotation per CLAUDE.md §9.
/// Refresh tokens stored hashed (SHA-256) — only the raw value is sent to client.
/// </summary>
public class JwtTokenService(IOptions<JwtSettings> options) : IJwtTokenService
{
    private readonly JwtSettings _settings = options.Value;

    public (string AccessToken, string RefreshToken, string RefreshTokenHash, DateTime AccessExpiresAt, DateTime RefreshExpiresAt)
        GenerateTokens(User user)
    {
        var accessExp = DateTime.UtcNow.AddMinutes(_settings.AccessTokenMinutes);
        var refreshExp = DateTime.UtcNow.AddDays(_settings.RefreshTokenDays);

        var access = BuildAccessToken(user, accessExp);
        var refresh = BuildRefreshToken();
        var refreshHash = HashRefreshToken(refresh);

        return (access, refresh, refreshHash, accessExp, refreshExp);
    }

    public string HashRefreshToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }

    private string BuildAccessToken(User user, DateTime expiresAt)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string BuildRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}
