using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Application.Features.Inventory.StoneParcels.DTOs;

public record ReceiveStoneParcelDto(
    string ParcelCode,
    Guid MaterialTypeId,
    decimal TotalCarat,
    int StoneCount,
    string? QualityGrade,
    StoneShape Shape,
    Guid? SupplierId,
    DateTime ReceivedDate,
    string Location,
    decimal UnitCostPerCarat,
    Currency CostCurrency,
    string? Notes
);

public record AdjustStoneParcelDto(
    decimal CaratDelta,
    int StoneCountDelta,
    string Reason,
    string? Notes,
    byte[] RowVersion
);

public record StoneParcelResponseDto(
    Guid Id,
    string ParcelCode,
    Guid MaterialTypeId,
    string MaterialTypeName,
    decimal TotalCarat,
    int StoneCount,
    decimal AverageSize,
    string? QualityGrade,
    string Shape,
    Guid? SupplierId,
    string? SupplierName,
    DateTime ReceivedDate,
    string Location,
    decimal UnitCostPerCarat,
    string CostCurrency,
    string Status,
    string? Notes,
    byte[] RowVersion,
    DateTime CreatedAt
);
