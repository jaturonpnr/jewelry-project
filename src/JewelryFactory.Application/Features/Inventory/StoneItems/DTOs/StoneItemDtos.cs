using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Application.Features.Inventory.StoneItems.DTOs;

public record ReceiveStoneItemDto(
    string ItemCode,
    Guid MaterialTypeId,
    decimal CaratWeight,
    StoneShape Shape,
    StoneColor? Color,
    StoneClarity? Clarity,
    string? Cut,
    string? Measurements,
    StoneCertAuthority CertAuthority,
    string? CertificateNumber,
    string? Origin,
    Guid? SupplierId,
    DateTime ReceivedDate,
    string Location,
    decimal UnitCost,
    Currency CostCurrency,
    string? Notes
);

public record UpdateStoneItemStatusDto(
    StockStatus Status,
    string? Notes,
    byte[] RowVersion
);

public record StoneItemResponseDto(
    Guid Id,
    string ItemCode,
    Guid MaterialTypeId,
    string MaterialTypeName,
    decimal CaratWeight,
    string Shape,
    string? Color,
    string? Clarity,
    string? Cut,
    string? Measurements,
    string CertAuthority,
    string? CertificateNumber,
    string? Origin,
    Guid? SupplierId,
    string? SupplierName,
    DateTime ReceivedDate,
    string Location,
    decimal UnitCost,
    string CostCurrency,
    string Status,
    string? Notes,
    byte[] RowVersion,
    DateTime CreatedAt
);
