using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Application.Features.Shipping.DTOs;

// ── Shipment Request DTOs ─────────────────────────────────────────────────────

public record ShipmentItemDto(
    string Description,
    string? DesignCode,
    int Quantity,
    decimal WeightGrams,
    decimal? UnitValueUsd,
    Guid? WorkOrderId
);

public record CreateShipmentDto(
    Guid? SalesOrderId,
    DateTime? ShipDate,
    DateTime? EstimatedDelivery,
    string? Carrier,
    string? TrackingNumber,
    string? ShippingMethod,
    string? PackingNotes,
    List<ShipmentItemDto> Items
);

public record UpdateShipmentStatusDto(
    ShipmentStatus NewStatus,
    DateTime? ActualDelivery,
    string? TrackingNumber
);

// ── Shipment Response DTOs ────────────────────────────────────────────────────

public record ShipmentItemResponse(
    Guid Id,
    string Description,
    string? DesignCode,
    int Quantity,
    decimal WeightGrams,
    decimal? UnitValueUsd,
    Guid? WorkOrderId,
    string? WorkOrderNumber
);

public record ShipmentResponse(
    Guid Id,
    string ShipmentNumber,
    Guid? SalesOrderId,
    string? SalesOrderNumber,
    ShipmentStatus Status,
    DateTime? ShipDate,
    DateTime? EstimatedDelivery,
    DateTime? ActualDelivery,
    string? Carrier,
    string? TrackingNumber,
    string? ShippingMethod,
    decimal TotalWeightGrams,
    string? PackingNotes,
    List<ShipmentItemResponse> Items,
    DateTime CreatedAt
);

public record ShipmentSummary(
    Guid Id,
    string ShipmentNumber,
    string? SalesOrderNumber,
    ShipmentStatus Status,
    DateTime? ShipDate,
    string? Carrier,
    string? TrackingNumber,
    int ItemCount,
    decimal TotalWeightGrams,
    DateTime CreatedAt
);

// ── Invoice Request DTOs ──────────────────────────────────────────────────────

public record InvoiceLineItemDto(
    int LineNumber,
    string Description,
    string? DesignCode,
    int Quantity,
    decimal UnitPrice,
    decimal DiscountPercent
);

public record CreateInvoiceDto(
    Guid CustomerId,
    Guid? SalesOrderId,
    Guid? ShipmentId,
    DateTime InvoiceDate,
    DateTime DueDate,
    Currency Currency,
    decimal ExchangeRateToThb,
    bool VatApplicable,
    string? PaymentTerms,
    string? Notes,
    List<InvoiceLineItemDto> LineItems
);

public record UpdateInvoiceStatusDto(
    InvoiceStatus NewStatus,
    DateTime? PaidAt,
    string? PaymentReference
);

// ── Invoice Response DTOs ─────────────────────────────────────────────────────

public record InvoiceLineItemResponse(
    Guid Id,
    int LineNumber,
    string Description,
    string? DesignCode,
    int Quantity,
    decimal UnitPrice,
    decimal DiscountPercent,
    decimal LineTotal
);

public record InvoiceResponse(
    Guid Id,
    string InvoiceNumber,
    Guid CustomerId,
    string CustomerName,
    Guid? SalesOrderId,
    string? SalesOrderNumber,
    Guid? ShipmentId,
    string? ShipmentNumber,
    InvoiceStatus Status,
    DateTime InvoiceDate,
    DateTime DueDate,
    Currency Currency,
    string CurrencyCode,
    decimal ExchangeRateToThb,
    bool VatApplicable,
    decimal VatPercent,
    string? PaymentTerms,
    string? Notes,
    DateTime? PaidAt,
    string? PaymentReference,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal VatAmount,
    decimal TotalAmount,
    decimal TotalAmountThb,
    List<InvoiceLineItemResponse> LineItems,
    DateTime CreatedAt
);

public record InvoiceSummary(
    Guid Id,
    string InvoiceNumber,
    string CustomerName,
    string? SalesOrderNumber,
    InvoiceStatus Status,
    DateTime InvoiceDate,
    DateTime DueDate,
    Currency Currency,
    decimal TotalAmount,
    decimal TotalAmountThb,
    DateTime CreatedAt
);
