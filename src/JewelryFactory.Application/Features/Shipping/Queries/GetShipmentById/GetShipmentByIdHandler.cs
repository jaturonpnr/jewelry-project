using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.Shipping.DTOs;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Shipping.Queries.GetShipmentById;

public class GetShipmentByIdHandler(IApplicationDbContext db)
    : IRequestHandler<GetShipmentByIdQuery, ShipmentResponse>
{
    public async Task<ShipmentResponse> Handle(GetShipmentByIdQuery query, CancellationToken ct)
    {
        var s = await db.Shipments.AsNoTracking()
            .Include(x => x.SalesOrder)
            .Include(x => x.Items).ThenInclude(i => i.WorkOrder)
            .FirstOrDefaultAsync(x => x.Id == query.Id && !x.IsDeleted, ct)
            ?? throw new NotFoundException(nameof(Shipment), query.Id);

        return new ShipmentResponse(
            s.Id, s.ShipmentNumber,
            s.SalesOrderId, s.SalesOrder?.OrderNumber,
            s.Status, s.ShipDate, s.EstimatedDelivery, s.ActualDelivery,
            s.Carrier, s.TrackingNumber, s.ShippingMethod,
            s.TotalWeightGrams, s.PackingNotes,
            s.Items.Select(i => new ShipmentItemResponse(
                i.Id, i.Description, i.DesignCode, i.Quantity,
                i.WeightGrams, i.UnitValueUsd,
                i.WorkOrderId, i.WorkOrder?.WorkOrderNumber))
            .ToList(),
            s.CreatedAt);
    }
}
