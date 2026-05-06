using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.Customers.Mappings; // AddressDto.ToValueObject()
using JewelryFactory.Application.Features.SalesOrders.DTOs;
using JewelryFactory.Application.Features.SalesOrders.Mappings;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Enums;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.SalesOrders.Commands.CreateSalesOrder;

public record CreateSalesOrderCommand(CreateSalesOrderDto Request) : IRequest<SalesOrderResponseDto>;

public class CreateSalesOrderHandler(IApplicationDbContext db)
    : IRequestHandler<CreateSalesOrderCommand, SalesOrderResponseDto>
{
    public async Task<SalesOrderResponseDto> Handle(CreateSalesOrderCommand command, CancellationToken ct)
    {
        var dto = command.Request;
        var orderNo = dto.OrderNumber.Trim().ToUpperInvariant();

        if (await db.SalesOrders.AnyAsync(o => o.OrderNumber == orderNo, ct))
            throw new BusinessRuleException($"Sales order number '{orderNo}' is already in use.");

        var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == dto.CustomerId, ct)
            ?? throw new NotFoundException(nameof(Customer), dto.CustomerId);

        if (!customer.IsActive)
            throw new BusinessRuleException("Cannot create order for an inactive customer.");

        if (dto.Items.Count == 0)
            throw new BusinessRuleException("Order must have at least one item.");

        // Build items
        var items = new List<SalesOrderItem>();
        var lineNo = 1;
        foreach (var item in dto.Items)
        {
            var lineTotal = (item.Quantity * item.UnitPrice) - item.LineDiscount;
            if (lineTotal < 0)
                throw new BusinessRuleException($"Line {lineNo}: discount exceeds (qty × unit price).");

            items.Add(new SalesOrderItem
            {
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

        var (subtotal, tax, total) = SalesOrderMapper.ComputeTotals(
            items, dto.DiscountAmount, dto.TaxRatePercent, dto.ShippingCost);

        var order = new SalesOrder
        {
            OrderNumber = orderNo,
            CustomerId = customer.Id,
            Currency = dto.Currency,
            ExchangeRateToBase = dto.ExchangeRateToBase,
            Status = SalesOrderStatus.Draft,
            OrderDate = dto.OrderDate,
            RequestedDeliveryDate = dto.RequestedDeliveryDate,
            DiscountAmount = dto.DiscountAmount,
            TaxAmount = tax,
            ShippingCost = dto.ShippingCost,
            Subtotal = subtotal,
            TotalAmount = total,
            ShippingAddress = dto.ShippingAddress.ToValueObject(),
            Notes = dto.Notes?.Trim(),
            Items = items
        };

        db.SalesOrders.Add(order);
        await db.SaveChangesAsync(ct);

        var saved = await db.SalesOrders
            .Include(x => x.Customer)
            .Include(x => x.Items)
            .FirstAsync(x => x.Id == order.Id, ct);
        return saved.ToResponse();
    }
}
