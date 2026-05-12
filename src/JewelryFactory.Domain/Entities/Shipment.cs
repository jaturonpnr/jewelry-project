using JewelryFactory.Domain.Common;
using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Domain.Entities;

/// <summary>
/// Physical shipment / packing list.
/// Phase 2 - Module 11.
/// </summary>
public class Shipment : AuditableEntity
{
    public required string ShipmentNumber { get; set; }   // SHP-2026-0001

    public Guid? SalesOrderId { get; set; }
    public SalesOrder? SalesOrder { get; set; }

    public ShipmentStatus Status { get; set; } = ShipmentStatus.Preparing;

    public DateTime? ShipDate { get; set; }
    public DateTime? EstimatedDelivery { get; set; }
    public DateTime? ActualDelivery { get; set; }

    public string? Carrier { get; set; }              // "DHL", "FedEx", "Thailand Post"
    public string? TrackingNumber { get; set; }
    public string? ShippingMethod { get; set; }       // "Air Freight", "Sea Freight", "Courier"

    public decimal TotalWeightGrams { get; set; }
    public string? PackingNotes { get; set; }

    public ICollection<ShipmentItem> Items { get; set; } = new List<ShipmentItem>();
}
