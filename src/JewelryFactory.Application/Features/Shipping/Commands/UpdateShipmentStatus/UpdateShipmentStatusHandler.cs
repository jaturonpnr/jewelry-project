using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Enums;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Shipping.Commands.UpdateShipmentStatus;

public class UpdateShipmentStatusHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateShipmentStatusCommand>
{
    public async Task Handle(UpdateShipmentStatusCommand command, CancellationToken ct)
    {
        var shipment = await db.Shipments
            .Include(s => s.SalesOrder)
            .FirstOrDefaultAsync(s => s.Id == command.Id && !s.IsDeleted, ct)
            ?? throw new NotFoundException(nameof(Shipment), command.Id);

        var req = command.Request;
        shipment.Status = req.NewStatus;

        if (req.TrackingNumber is not null)
            shipment.TrackingNumber = req.TrackingNumber.Trim();

        if (req.NewStatus == ShipmentStatus.Shipped)
            shipment.ShipDate ??= DateTime.UtcNow;

        if (req.NewStatus == ShipmentStatus.Delivered)
        {
            shipment.ActualDelivery = req.ActualDelivery?.ToUniversalTime() ?? DateTime.UtcNow;

            // Advance linked Sales Order to Delivered if currently Shipped
            if (shipment.SalesOrder is { Status: SalesOrderStatus.Shipped })
                shipment.SalesOrder.Status = SalesOrderStatus.Delivered;
        }

        await db.SaveChangesAsync(ct);
    }
}
