using JewelryFactory.Application.Features.Shipping.DTOs;
using MediatR;

namespace JewelryFactory.Application.Features.Shipping.Commands.CreateShipment;

public record CreateShipmentCommand(CreateShipmentDto Request) : IRequest<Guid>;
