using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.SalesOrders.DTOs;
using JewelryFactory.Application.Features.SalesOrders.Mappings;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Enums;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JewelryFactory.Application.Features.SalesOrders.Commands.ChangeSalesOrderStatus;

/// <summary>
/// Transition a sales order through its lifecycle, enforcing CLAUDE.md §10.9
/// and triggering inventory side-effects (reserve / release / mark sold).
/// </summary>
public record ChangeSalesOrderStatusCommand(Guid Id, ChangeSalesOrderStatusDto Request)
    : IRequest<SalesOrderResponseDto>;

public class ChangeSalesOrderStatusHandler(
    IApplicationDbContext db,
    IStockMovementWriter movements,
    ILogger<ChangeSalesOrderStatusHandler> logger
) : IRequestHandler<ChangeSalesOrderStatusCommand, SalesOrderResponseDto>
{
    public async Task<SalesOrderResponseDto> Handle(ChangeSalesOrderStatusCommand command, CancellationToken ct)
    {
        var order = await db.SalesOrders
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(i => i.FinishedGoods)
            .FirstOrDefaultAsync(x => x.Id == command.Id, ct)
            ?? throw new NotFoundException(nameof(SalesOrder), command.Id);

        var dto = command.Request;
        var oldStatus = order.Status;
        var newStatus = dto.NewStatus;

        if (oldStatus == newStatus)
            return order.ToResponse();

        if (!SalesOrderStatusFlow.CanTransition(oldStatus, newStatus))
            throw new BusinessRuleException(
                $"Invalid transition: {oldStatus} → {newStatus}.");

        if (newStatus == SalesOrderStatus.Cancelled && string.IsNullOrWhiteSpace(dto.Reason))
            throw new BusinessRuleException("Cancellation reason is required.");

        // Side-effects ─────────────────────────────────────────────────────────
        switch (newStatus)
        {
            case SalesOrderStatus.Confirmed:
                ReserveLinkedFinishedGoods(order);
                order.ConfirmedAt = DateTime.UtcNow;
                break;

            case SalesOrderStatus.Shipped:
                MarkLinkedFinishedGoodsSold(order);
                order.ShippedAt = DateTime.UtcNow;
                if (!string.IsNullOrWhiteSpace(dto.TrackingNumber))
                    order.TrackingNumber = dto.TrackingNumber.Trim();
                break;

            case SalesOrderStatus.Delivered:
                order.DeliveredAt = DateTime.UtcNow;
                break;

            case SalesOrderStatus.Cancelled:
                ReleaseReservedFinishedGoods(order);
                order.CancelledAt = DateTime.UtcNow;
                order.CancellationReason = dto.Reason!.Trim();
                break;
        }

        order.Status = newStatus;
        db.Entry(order).Property(x => x.RowVersion).OriginalValue = dto.RowVersion;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException(nameof(SalesOrder), command.Id);
        }

        logger.LogInformation(
            "SalesOrder {OrderId} {OrderNumber}: {OldStatus} → {NewStatus}",
            order.Id, order.OrderNumber, oldStatus, newStatus);

        return order.ToResponse();
    }

    // ── Side-effect helpers ───────────────────────────────────────────────────

    private void ReserveLinkedFinishedGoods(SalesOrder order)
    {
        foreach (var item in order.Items.Where(i => i.FinishedGoods != null))
        {
            var fg = item.FinishedGoods!;
            if (fg.Status != StockStatus.InStock)
                throw new BusinessRuleException(
                    $"FinishedGoods '{fg.SerialNumber}' is not InStock (current: {fg.Status}).");

            fg.Status = StockStatus.Reserved;
            movements.Record(
                InventoryItemType.FinishedGoods,
                fg.Id,
                StockMovementType.Reserve,
                quantityDelta: 0,
                quantityAfter: 1,
                referenceType: "SalesOrder",
                referenceId: order.Id,
                reason: $"Reserved for order {order.OrderNumber}");
        }
    }

    private void ReleaseReservedFinishedGoods(SalesOrder order)
    {
        foreach (var item in order.Items.Where(i => i.FinishedGoods != null))
        {
            var fg = item.FinishedGoods!;
            if (fg.Status == StockStatus.Reserved)
            {
                fg.Status = StockStatus.InStock;
                movements.Record(
                    InventoryItemType.FinishedGoods,
                    fg.Id,
                    StockMovementType.Release,
                    quantityDelta: 0,
                    quantityAfter: 1,
                    referenceType: "SalesOrder",
                    referenceId: order.Id,
                    reason: $"Released — order {order.OrderNumber} cancelled");
            }
        }
    }

    private void MarkLinkedFinishedGoodsSold(SalesOrder order)
    {
        foreach (var item in order.Items.Where(i => i.FinishedGoods != null))
        {
            var fg = item.FinishedGoods!;
            fg.Status = StockStatus.Sold;
            movements.Record(
                InventoryItemType.FinishedGoods,
                fg.Id,
                StockMovementType.Issue,
                quantityDelta: -1,
                quantityAfter: 0,
                referenceType: "SalesOrder",
                referenceId: order.Id,
                reason: $"Shipped — order {order.OrderNumber}");
        }
    }
}
