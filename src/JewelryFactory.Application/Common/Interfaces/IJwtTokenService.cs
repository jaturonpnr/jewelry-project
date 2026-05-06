using JewelryFactory.Domain.Entities;

namespace JewelryFactory.Application.Common.Interfaces;

public interface IJwtTokenService
{
    /// <summary>
    /// Generates an access token (JWT) and a refresh token (raw + hash).
    /// </summary>
    (string AccessToken, string RefreshToken, string RefreshTokenHash, DateTime AccessExpiresAt, DateTime RefreshExpiresAt)
        GenerateTokens(User user);

    string HashRefreshToken(string rawToken);
}
