using JewelryFactory.Application.Features.Customers.DTOs;
using JewelryFactory.Application.Features.SalesOrders.DTOs;
using JewelryFactory.Domain.Entities;

namespace JewelryFactory.Application.Features.SalesOrders.Mappings;

internal static class SalesOrderMapper
{
    public static SalesOrderResponseDto ToResponse(this SalesOrder o) => new(
        o.Id,
        o.OrderNumber,
        o.CustomerId,
        o.Customer?.Code ?? "",
        o.Customer?.CompanyName ?? "",
        o.Currency.ToString(),
        o.ExchangeRateToBase,
        o.Status.ToString(),
        o.OrderDate,
        o.RequestedDeliveryDate,
        o.ConfirmedAt,
        o.ShippedAt,
        o.DeliveredAt,
        o.CancelledAt,
        o.CancellationReason,
        o.Subtotal,
        o.DiscountAmount,
        o.TaxAmount,
        o.ShippingCost,
        o.TotalAmount,
        new AddressDto(
            o.ShippingAddress.Line1, o.ShippingAddress.Line2,
            o.ShippingAddress.City, o.ShippingAddress.State,
            o.ShippingAddress.PostalCode, o.ShippingAddress.Country),
        o.TrackingNumber,
        o.Notes,
        o.Items.OrderBy(i => i.LineNumber).Select(i => new SalesOrderItemResponseDto(
            i.Id, i.LineNumber, i.Description, i.DesignCode, i.FinishedGoodsId,
            i.Quantity, i.UnitPrice, i.LineDiscount, i.LineTotal, i.Notes
        )).ToList(),
        o.RowVersion,
        o.CreatedAt
    );

    public static (decimal Subtotal, decimal Tax, decimal Total)
        ComputeTotals(IEnumerable<SalesOrderItem> items, decimal discount, decimal taxRatePercent, decimal shipping)
    {
        var subtotal = items.Sum(i => i.LineTotal);
        var taxable = Math.Max(0, subtotal - discount);
        var tax = Math.Round(taxable * taxRatePercent / 100m, 2);
        var total = taxable + tax + shipping;
        return (subtotal, tax, total);
    }
}
