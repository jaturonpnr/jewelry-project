using JewelryFactory.Application.Features.Shipping.DTOs;
using MediatR;

namespace JewelryFactory.Application.Features.Shipping.Commands.UpdateShipmentStatus;

public record UpdateShipmentStatusCommand(Guid Id, UpdateShipmentStatusDto Request) : IRequest;
