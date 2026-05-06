using JewelryFactory.Application.Features.Customers.DTOs; // AddressDto
using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Application.Features.SalesOrders.DTOs;

public record CreateSalesOrderItemDto(
    string Description,
    string? DesignCode,
    Guid? FinishedGoodsId,
    int Quantity,
    decimal UnitPrice,
    decimal LineDiscount,
    string? Notes
);

public record CreateSalesOrderDto(
    string OrderNumber,
    Guid CustomerId,
    Currency Currency,
    decimal? ExchangeRateToBase,
    DateTime OrderDate,
    DateTime? RequestedDeliveryDate,
    decimal DiscountAmount,
    decimal TaxRatePercent,            // e.g. 7 for Thai VAT — applied to (subtotal - discount)
    decimal ShippingCost,
    AddressDto ShippingAddress,
    string? Notes,
    List<CreateSalesOrderItemDto> Items
);

public record UpdateSalesOrderDto(
    DateTime OrderDate,
    DateTime? RequestedDeliveryDate,
    decimal DiscountAmount,
    decimal TaxRatePercent,
    decimal ShippingCost,
    AddressDto ShippingAddress,
    string? Notes,
    List<CreateSalesOrderItemDto> Items,
    byte[] RowVersion
);

public record ChangeSalesOrderStatusDto(
    SalesOrderStatus NewStatus,
    string? Reason,                    // required when cancelling
    string? TrackingNumber,            // optional when shipping
    byte[] RowVersion
);

public record SalesOrderItemResponseDto(
    Guid Id,
    int LineNumber,
    string Description,
    string? DesignCode,
    Guid? FinishedGoodsId,
    int Quantity,
    decimal UnitPrice,
    decimal LineDiscount,
    decimal LineTotal,
    string? Notes
);

public record SalesOrderResponseDto(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    string CustomerCode,
    string CustomerName,
    string Currency,
    decimal? ExchangeRateToBase,
    string Status,
    DateTime OrderDate,
    DateTime? RequestedDeliveryDate,
    DateTime? ConfirmedAt,
    DateTime? ShippedAt,
    DateTime? DeliveredAt,
    DateTime? CancelledAt,
    string? CancellationReason,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal ShippingCost,
    decimal TotalAmount,
    AddressDto ShippingAddress,
    string? TrackingNumber,
    string? Notes,
    List<SalesOrderItemResponseDto> Items,
    byte[] RowVersion,
    DateTime CreatedAt
);
