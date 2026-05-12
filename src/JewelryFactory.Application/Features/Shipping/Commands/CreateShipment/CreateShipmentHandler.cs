using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Shipping.Commands.CreateShipment;

public class CreateShipmentHandler(IApplicationDbContext db)
    : IRequestHandler<CreateShipmentCommand, Guid>
{
    public async Task<Guid> Handle(CreateShipmentCommand command, CancellationToken ct)
    {
        var req = command.Request;

        var number = await GenerateShipmentNumberAsync(ct);

        var shipment = new Shipment
        {
            ShipmentNumber = number,
            SalesOrderId = req.SalesOrderId,
            ShipDate = req.ShipDate?.ToUniversalTime(),
            EstimatedDelivery = req.EstimatedDelivery?.ToUniversalTime(),
            Carrier = req.Carrier?.Trim(),
            TrackingNumber = req.TrackingNumber?.Trim(),
            ShippingMethod = req.ShippingMethod?.Trim(),
            PackingNotes = req.PackingNotes?.Trim(),
            TotalWeightGrams = req.Items.Sum(i => i.WeightGrams * i.Quantity),
        };

        foreach (var item in req.Items)
        {
            shipment.Items.Add(new ShipmentItem
            {
                Description = item.Description.Trim(),
                DesignCode = item.DesignCode?.Trim(),
                Quantity = item.Quantity,
                WeightGrams = item.WeightGrams,
                UnitValueUsd = item.UnitValueUsd,
                WorkOrderId = item.WorkOrderId,
            });
        }

        db.Shipments.Add(shipment);
        await db.SaveChangesAsync(ct);
        return shipment.Id;
    }

    private async Task<string> GenerateShipmentNumberAsync(CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var count = await db.Shipments.CountAsync(ct) + 1;
        return $"SHP-{year}-{count:D4}";
    }
}
