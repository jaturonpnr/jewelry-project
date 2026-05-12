using JewelryFactory.Domain.Common;

namespace JewelryFactory.Domain.Entities;

public class ShipmentItem : BaseEntity
{
    public Guid ShipmentId { get; set; }
    public Shipment Shipment { get; set; } = null!;

    public required string Description { get; set; }
    public string? DesignCode { get; set; }
    public int Quantity { get; set; }
    public decimal WeightGrams { get; set; }          // gross weight per line
    public decimal? UnitValueUsd { get; set; }        // declared customs value

    public Guid? WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }
}
