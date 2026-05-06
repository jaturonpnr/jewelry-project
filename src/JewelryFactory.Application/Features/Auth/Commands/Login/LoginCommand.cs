using JewelryFactory.Application.Features.Auth.DTOs;
using MediatR;

namespace JewelryFactory.Application.Features.Auth.Commands.Login;

public record LoginCommand(LoginRequestDto Request) : IRequest<AuthResponseDto>;
