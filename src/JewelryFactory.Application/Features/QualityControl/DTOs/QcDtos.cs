using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Application.Features.QualityControl.DTOs;

// ── Request DTOs ──────────────────────────────────────────────────────────────

public record QcDefectDto(
    string DefectType,
    DefectSeverity Severity,
    string? Description,
    int Quantity
);

public record CreateQcInspectionDto(
    QcInspectionType InspectionType,
    DateTime InspectionDate,
    Guid? WorkOrderId,
    Guid? WorkOrderStageId,
    Guid? RawMaterialItemId,
    Guid? InspectorId,
    string InspectorName,
    QcResult Result,
    string? Notes,
    ProductionStage? ReworkStage,
    decimal? ActualWeightGrams,
    decimal? ExpectedWeightGrams,
    List<QcDefectDto> Defects
);

// ── Response DTOs ─────────────────────────────────────────────────────────────

public record QcDefectResponse(
    Guid Id,
    string DefectType,
    DefectSeverity Severity,
    string? Description,
    int Quantity
);

public record QcInspectionResponse(
    Guid Id,
    QcInspectionType InspectionType,
    DateTime InspectionDate,
    Guid? WorkOrderId,
    string? WorkOrderNumber,
    Guid? WorkOrderStageId,
    string? WorkOrderStageName,
    Guid? RawMaterialItemId,
    string? RawMaterialDescription,
    Guid? InspectorId,
    string InspectorName,
    QcResult Result,
    string? Notes,
    ProductionStage? ReworkStage,
    decimal? ActualWeightGrams,
    decimal? ExpectedWeightGrams,
    int DefectCount,
    List<QcDefectResponse> Defects,
    DateTime CreatedAt
);

public record QcInspectionSummary(
    Guid Id,
    QcInspectionType InspectionType,
    DateTime InspectionDate,
    string? WorkOrderNumber,
    string? RawMaterialDescription,
    string InspectorName,
    QcResult Result,
    int DefectCount,
    DateTime CreatedAt
);
