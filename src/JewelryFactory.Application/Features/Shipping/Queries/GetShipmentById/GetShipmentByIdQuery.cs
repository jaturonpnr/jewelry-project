using JewelryFactory.Application.Features.Shipping.DTOs;
using MediatR;

namespace JewelryFactory.Application.Features.Shipping.Queries.GetShipmentById;

public record GetShipmentByIdQuery(Guid Id) : IRequest<ShipmentResponse>;
