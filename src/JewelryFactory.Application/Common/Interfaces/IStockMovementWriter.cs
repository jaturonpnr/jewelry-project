using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Application.Common.Interfaces;

/// <summary>
/// Records inventory changes as immutable StockMovement audit rows.
/// Use whenever quantity or status of an inventory item changes.
/// </summary>
public interface IStockMovementWriter
{
    void Record(
        InventoryItemType itemType,
        Guid itemId,
        StockMovementType movementType,
        decimal quantityDelta,
        decimal quantityAfter,
        string? referenceType = null,
        Guid? referenceId = null,
        string? reason = null,
        string? notes = null);
}
