using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Application.Features.Inventory.RawMaterials.DTOs;

public record ReceiveRawMaterialDto(
    Guid MaterialTypeId,
    string LotNumber,
    string Location,
    decimal Quantity,
    decimal UnitCost,
    Currency CostCurrency,
    Guid? SupplierId,
    DateTime ReceivedDate,
    string? Notes
);

public record AdjustRawMaterialDto(
    decimal QuantityDelta,         // signed; positive = add, negative = subtract
    string Reason,
    string? Notes,
    byte[] RowVersion              // optimistic concurrency token
);

public record RawMaterialResponseDto(
    Guid Id,
    Guid MaterialTypeId,
    string MaterialTypeCode,
    string MaterialTypeName,
    string MaterialUnit,
    string LotNumber,
    string Location,
    decimal Quantity,
    decimal? PureWeight,
    decimal UnitCost,
    string CostCurrency,
    Guid? SupplierId,
    string? SupplierName,
    DateTime ReceivedDate,
    string Status,
    string? Notes,
    byte[] RowVersion,
    DateTime CreatedAt
);
