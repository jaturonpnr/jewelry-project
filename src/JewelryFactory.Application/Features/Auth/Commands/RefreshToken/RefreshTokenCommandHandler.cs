using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.Auth.DTOs;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RefreshTokenEntity = JewelryFactory.Domain.Entities.RefreshToken;

namespace JewelryFactory.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler(
    IApplicationDbContext db,
    IJwtTokenService jwtService,
    ILogger<RefreshTokenCommandHandler> logger
) : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(RefreshTokenCommand command, CancellationToken ct)
    {
        var rawToken = command.Request.RefreshToken;
        if (string.IsNullOrWhiteSpace(rawToken))
            throw new UnauthorizedException("Refresh token is required.");

        var tokenHash = jwtService.HashRefreshToken(rawToken);

        var existing = await db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, ct);

        if (existing is null || !existing.IsActive)
        {
            logger.LogWarning("Refresh token rejected — invalid or expired.");
            throw new UnauthorizedException("Invalid refresh token.");
        }

        if (existing.User.IsDeleted || !existing.User.IsActive)
            throw new UnauthorizedException("User is not active.");

        // Rotate: revoke old, issue new
        var (access, refresh, refreshHash, accessExp, refreshExp) = jwtService.GenerateTokens(existing.User);

        existing.RevokedAt = DateTime.UtcNow;
        existing.ReplacedByTokenHash = refreshHash;

        db.RefreshTokens.Add(new RefreshTokenEntity
        {
            UserId = existing.UserId,
            TokenHash = refreshHash,
            ExpiresAt = refreshExp
        });

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Refresh token rotated for user: {UserId}", existing.UserId);

        var u = existing.User;
        return new AuthResponseDto(
            u.Id, u.Email, u.FullName, u.Role.ToString(),
            access, refresh, accessExp, refreshExp
        );
    }
}
