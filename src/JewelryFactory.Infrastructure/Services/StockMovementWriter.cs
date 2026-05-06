using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Infrastructure.Services;

public class StockMovementWriter(
    IApplicationDbContext db,
    ICurrentUserService currentUser
) : IStockMovementWriter
{
    public void Record(
        InventoryItemType itemType,
        Guid itemId,
        StockMovementType movementType,
        decimal quantityDelta,
        decimal quantityAfter,
        string? referenceType = null,
        Guid? referenceId = null,
        string? reason = null,
        string? notes = null)
    {
        db.StockMovements.Add(new StockMovement
        {
            ItemType = itemType,
            ItemId = itemId,
            MovementType = movementType,
            QuantityDelta = quantityDelta,
            QuantityAfter = quantityAfter,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            Reason = reason,
            Notes = notes,
            PerformedBy = currentUser.UserId?.ToString() ?? "system",
            PerformedAt = DateTime.UtcNow
        });
    }
}
