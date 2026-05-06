using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.Customers.Mappings;
using JewelryFactory.Application.Features.SalesOrders.DTOs;
using JewelryFactory.Application.Features.SalesOrders.Mappings;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Enums;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.SalesOrders.Commands.UpdateSalesOrder;

/// <summary>
/// Update an order — only allowed while in Draft status.
/// </summary>
public record UpdateSalesOrderCommand(Guid Id, UpdateSalesOrderDto Request) : IRequest<SalesOrderResponseDto>;

public class UpdateSalesOrderHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateSalesOrderCommand, SalesOrderResponseDto>
{
    public async Task<SalesOrderResponseDto> Handle(UpdateSalesOrderCommand command, CancellationToken ct)
    {
        var order = await db.SalesOrders
            .Include(x => x.Customer)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == command.Id, ct)
            ?? throw new NotFoundException(nameof(SalesOrder), command.Id);

        if (order.Status != SalesOrderStatus.Draft)
            throw new BusinessRuleException(
                $"Order can only be edited in Draft status (current: {order.Status}).");

        var dto = command.Request;
        if (dto.Items.Count == 0)
            throw new BusinessRuleException("Order must have at least one item.");

        // Replace items entirely
        foreach (var existing in order.Items.ToList())
            db.SalesOrderItems.Remove(existing);
        order.Items.Clear();

        var lineNo = 1;
        foreach (var item in dto.Items)
        {
            var lineTotal = (item.Quantity * item.UnitPrice) - item.LineDiscount;
            if (lineTotal < 0)
                throw new BusinessRuleException($"Line {lineNo}: discount exceeds (qty × unit price).");

            order.Items.Add(new SalesOrderItem
            {
                SalesOrderId = order.Id,
                LineNumber = lineNo++,
                Description = item.Description.Trim(),
                DesignCode = item.DesignCode?.Trim(),
                FinishedGoodsId = item.FinishedGoodsId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineDiscount = item.LineDiscount,
                LineTotal = lineTotal,
                Notes = item.Notes?.Trim()
            });
        }

        order.OrderDate = dto.OrderDate;
        order.RequestedDeliveryDate = dto.RequestedDeliveryDate;
        order.DiscountAmount = dto.DiscountAmount;
        order.ShippingCost = dto.ShippingCost;
        order.ShippingAddress = dto.ShippingAddress.ToValueObject();
        order.Notes = dto.Notes?.Trim();

        var (subtotal, tax, total) = SalesOrderMapper.ComputeTotals(
            order.Items, dto.DiscountAmount, dto.TaxRatePercent, dto.ShippingCost);
        order.Subtotal = subtotal;
        order.TaxAmount = tax;
        order.TotalAmount = total;

        db.Entry(order).Property(x => x.RowVersion).OriginalValue = dto.RowVersion;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException(nameof(SalesOrder), command.Id);
        }

        return order.ToResponse();
    }
}
