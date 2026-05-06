using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Application.Features.Auth.DTOs;

public record RegisterRequestDto(
    string Email,
    string Password,
    string FullName,
    UserRole Role
);

public record LoginRequestDto(
    string Email,
    string Password
);

public record RefreshTokenRequestDto(
    string RefreshToken
);

public record AuthResponseDto(
    Guid UserId,
    string Email,
    string FullName,
    string Role,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt
);

public record UserResponseDto(
    Guid Id,
    string Email,
    string FullName,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);
