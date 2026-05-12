using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.Shipping.DTOs;
using JewelryFactory.Domain.Enums;
using MediatR;

namespace JewelryFactory.Application.Features.Shipping.Queries.GetShipmentsPaged;

public record GetShipmentsPagedQuery(
    int Page, int PageSize,
    ShipmentStatus? Status,
    string? Search
) : IRequest<PagedResult<ShipmentSummary>>;
