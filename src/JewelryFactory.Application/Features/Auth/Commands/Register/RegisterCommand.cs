using JewelryFactory.Application.Features.Auth.DTOs;
using MediatR;

namespace JewelryFactory.Application.Features.Auth.Commands.Register;

public record RegisterCommand(RegisterRequestDto Request) : IRequest<AuthResponseDto>;
