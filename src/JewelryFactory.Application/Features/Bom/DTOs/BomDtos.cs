using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Application.Features.Bom.DTOs;

// ── Request DTOs ─────────────────────────────────────────────────────────────

public record BomMaterialLineDto(
    Guid? Id,
    MaterialCategory Category,
    string MaterialDescription,
    int? Karat,
    decimal? PurityFraction,
    decimal QuantityGrams,
    decimal ExpectedLossPercent,
    decimal UnitCostThbPerGram,
    int SortOrder
);

public record BomStoneLineDto(
    Guid? Id,
    string StoneType,
    string StoneShape,
    string SizeDescription,
    decimal CaratPerStone,
    int Quantity,
    StoneTrackingType TrackingType,
    decimal UnitCostThbPerCarat,
    int SortOrder
);

public record BomLaborLineDto(
    Guid? Id,
    ProductionStage Stage,
    decimal EstimatedHours,
    decimal HourlyRateThb
);

public record CreateBomTemplateDto(
    string DesignCode,
    string DesignName,
    string? Description,
    decimal OverheadPercent,
    List<BomMaterialLineDto> MaterialLines,
    List<BomStoneLineDto> StoneLines,
    List<BomLaborLineDto> LaborLines
);

public record UpdateBomTemplateDto(
    string DesignCode,
    string DesignName,
    string? Description,
    decimal OverheadPercent,
    bool IsActive,
    List<BomMaterialLineDto> MaterialLines,
    List<BomStoneLineDto> StoneLines,
    List<BomLaborLineDto> LaborLines
);

// ── Response DTOs ─────────────────────────────────────────────────────────────

public record BomMaterialLineResponse(
    Guid Id,
    MaterialCategory Category,
    string MaterialDescription,
    int? Karat,
    decimal? PurityFraction,
    decimal QuantityGrams,
    decimal ExpectedLossPercent,
    decimal UnitCostThbPerGram,
    decimal LineCostThb,
    int SortOrder
);

public record BomStoneLineResponse(
    Guid Id,
    string StoneType,
    string StoneShape,
    string SizeDescription,
    decimal CaratPerStone,
    int Quantity,
    decimal TotalCaratWeight,
    StoneTrackingType TrackingType,
    decimal UnitCostThbPerCarat,
    decimal LineCostThb,
    int SortOrder
);

public record BomLaborLineResponse(
    Guid Id,
    ProductionStage Stage,
    string StageName,
    decimal EstimatedHours,
    decimal HourlyRateThb,
    decimal LaborCostThb
);

public record BomCostSummary(
    decimal MaterialCostThb,
    decimal StoneCostThb,
    decimal LaborCostThb,
    decimal SubtotalThb,
    decimal OverheadPercent,
    decimal OverheadCostThb,
    decimal TotalCostPerPieceThb
);

public record BomTemplateResponse(
    Guid Id,
    string DesignCode,
    string DesignName,
    string? Description,
    bool IsActive,
    decimal OverheadPercent,
    List<BomMaterialLineResponse> MaterialLines,
    List<BomStoneLineResponse> StoneLines,
    List<BomLaborLineResponse> LaborLines,
    BomCostSummary CostSummary,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record BomTemplateSummaryResponse(
    Guid Id,
    string DesignCode,
    string DesignName,
    bool IsActive,
    int MaterialLineCount,
    int StoneLineCount,
    int LaborLineCount,
    decimal TotalCostPerPieceThb,
    DateTime CreatedAt
);
