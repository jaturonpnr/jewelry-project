namespace JewelryFactory.Application.Features.Reports.DTOs;

public record InventorySummaryReportDto(
    InventoryTotalsDto Totals,
    IReadOnlyList<RawMaterialByCategoryDto> RawMaterialsByCategory,
    IReadOnlyList<RawMaterialByMaterialDto> RawMaterialsByMaterial,
    IReadOnlyList<StockByStatusDto> RawMaterialsByStatus,
    IReadOnlyList<StockByStatusDto> StoneItemsByStatus,
    IReadOnlyList<StockByStatusDto> StoneParcelsByStatus
);

public record InventoryTotalsDto(
    int RawMaterialLotCount,
    int StoneItemCount,
    int StoneParcelCount,
    decimal TotalPureGoldGrams,        // sum of pureWeight across all gold lots InStock
    decimal TotalStoneCarat,           // individual + parcel
    int TotalStoneCount                // individual + parcel.stoneCount
);

public record RawMaterialByCategoryDto(
    string Category,                   // Metal / Finding / Consumable
    int LotCount,
    decimal TotalQuantity              // sum of Quantity (mixed units — for display only)
);

public record RawMaterialByMaterialDto(
    string MaterialCode,
    string MaterialName,
    string Unit,
    int LotCount,
    decimal TotalQuantity,
    decimal? TotalPureWeight           // null for non-metals
);

public record StockByStatusDto(
    string Status,
    int Count
);
