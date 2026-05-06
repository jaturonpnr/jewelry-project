using JewelryFactory.Application.Features.Auth.DTOs;
using MediatR;

namespace JewelryFactory.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(RefreshTokenRequestDto Request) : IRequest<AuthResponseDto>;
