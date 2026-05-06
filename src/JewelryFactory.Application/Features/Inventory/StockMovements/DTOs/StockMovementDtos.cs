namespace JewelryFactory.Application.Features.Inventory.StockMovements.DTOs;

public record StockMovementResponseDto(
    Guid Id,
    string ItemType,
    Guid ItemId,
    string MovementType,
    decimal QuantityDelta,
    decimal QuantityAfter,
    string? ReferenceType,
    Guid? ReferenceId,
    string? Reason,
    string? Notes,
    string PerformedBy,
    DateTime PerformedAt
);
