using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.Auth.DTOs;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RefreshTokenEntity = JewelryFactory.Domain.Entities.RefreshToken;

namespace JewelryFactory.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtService,
    ILogger<LoginCommandHandler> logger
) : IRequestHandler<LoginCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(LoginCommand command, CancellationToken ct)
    {
        var req = command.Request;
        var emailNormalized = req.Email.Trim().ToLowerInvariant();

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == emailNormalized && !u.IsDeleted, ct);

        if (user is null || !user.IsActive)
        {
            logger.LogWarning("Login failed — user not found or inactive: {Email}", emailNormalized);
            throw new UnauthorizedException("Invalid email or password.");
        }

        if (!passwordHasher.Verify(req.Password, user.PasswordHash))
        {
            logger.LogWarning("Login failed — wrong password for: {Email}", emailNormalized);
            throw new UnauthorizedException("Invalid email or password.");
        }

        var (access, refresh, refreshHash, accessExp, refreshExp) = jwtService.GenerateTokens(user);

        db.RefreshTokens.Add(new RefreshTokenEntity
        {
            UserId = user.Id,
            TokenHash = refreshHash,
            ExpiresAt = refreshExp
        });

        user.LastLoginAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("User logged in: {UserId}", user.Id);

        return new AuthResponseDto(
            user.Id,
            user.Email,
            user.FullName,
            user.Role.ToString(),
            access,
            refresh,
            accessExp,
            refreshExp
        );
    }
}
