using JewelryFactory.Domain.Common;
using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Domain.Entities;

/// <summary>
/// Audit-trail of every inventory change (immutable once written).
/// Polymorphic by ItemType + ItemId (no FK relationship — kept simple).
/// </summary>
public class StockMovement : BaseEntity
{
    public required InventoryItemType ItemType { get; set; }
    public required Guid ItemId { get; set; }
    public required StockMovementType MovementType { get; set; }

    /// <summary>
    /// Quantity change (signed): positive = increase, negative = decrease.
    /// Unit depends on ItemType (gram/carat/piece).
    /// </summary>
    public decimal QuantityDelta { get; set; }

    public decimal QuantityAfter { get; set; }       // snapshot of quantity after this move

    public string? ReferenceType { get; set; }        // e.g. "SalesOrder", "WorkOrder", "Adjustment"
    public Guid? ReferenceId { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }

    public required string PerformedBy { get; set; }
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
}
