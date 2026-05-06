using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.Auth.DTOs;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RefreshTokenEntity = JewelryFactory.Domain.Entities.RefreshToken;

namespace JewelryFactory.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtService,
    ILogger<RegisterCommandHandler> logger
) : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(RegisterCommand command, CancellationToken ct)
    {
        var req = command.Request;
        var emailNormalized = req.Email.Trim().ToLowerInvariant();

        var emailExists = await db.Users
            .AnyAsync(u => u.Email == emailNormalized && !u.IsDeleted, ct);

        if (emailExists)
        {
            logger.LogWarning("Register failed — email already in use: {Email}", emailNormalized);
            throw new BusinessRuleException("Email is already registered.");
        }

        var user = new User
        {
            Email = emailNormalized,
            PasswordHash = passwordHasher.Hash(req.Password),
            FullName = req.FullName.Trim(),
            Role = req.Role,
            IsActive = true,
            CreatedBy = "self-register"
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("User registered: {UserId} ({Email})", user.Id, user.Email);

        return await IssueTokensAsync(user, ct);
    }

    private async Task<AuthResponseDto> IssueTokensAsync(User user, CancellationToken ct)
    {
        var (access, refresh, refreshHash, accessExp, refreshExp) = jwtService.GenerateTokens(user);

        db.RefreshTokens.Add(new RefreshTokenEntity
        {
            UserId = user.Id,
            TokenHash = refreshHash,
            ExpiresAt = refreshExp
        });

        user.LastLoginAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

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
