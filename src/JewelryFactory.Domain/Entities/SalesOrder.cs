using JewelryFactory.Domain.Common;
using JewelryFactory.Domain.Enums;
using JewelryFactory.Domain.ValueObjects;

namespace JewelryFactory.Domain.Entities;

/// <summary>
/// B2B sales order header. Status flow enforced — see SalesOrderStatusFlow.
/// Phase 1 - Module 4.
/// </summary>
public class SalesOrder : ConcurrentEntity
{
    public required string OrderNumber { get; set; }       // unique, e.g. SO-2026-0001

    public required Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public Currency Currency { get; set; } = Currency.USD;
    public decimal? ExchangeRateToBase { get; set; }       // optional snapshot vs THB

    public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Draft;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? RequestedDeliveryDate { get; set; }

    // Status timestamps
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    // Financials (computed from items + adjustments)
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }                  // VAT 7% for THB orders
    public decimal ShippingCost { get; set; }
    public decimal TotalAmount { get; set; }

    public Address ShippingAddress { get; set; } = new();
    public string? TrackingNumber { get; set; }
    public string? Notes { get; set; }

    public ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();
}
