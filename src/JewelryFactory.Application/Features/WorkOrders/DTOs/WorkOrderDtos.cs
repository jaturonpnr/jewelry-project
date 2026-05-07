using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Application.Features.WorkOrders.DTOs;

// ── Stage estimate (used at create time to pre-fill all 9 stages) ────────
public record CreateStageEstimateDto(
    ProductionStage Stage,
    decimal EstimatedHours,
    Guid? AssignedWorkerId
);

public record CreateWorkOrderDto(
    string WorkOrderNumber,
    Guid? SalesOrderId,
    string? DesignCode,
    string Description,
    int Quantity,
    WorkOrderPriority Priority,
    DateTime? ScheduledStart,
    DateTime? ScheduledEnd,
    Guid? AssignedSupervisorId,
    string? Notes,
    /// <summary>
    /// Optional per-stage estimates. Any stage not provided is created with 0 hours.
    /// </summary>
    List<CreateStageEstimateDto>? StageEstimates
);

public record UpdateWorkOrderDto(
    string? DesignCode,
    string Description,
    int Quantity,
    WorkOrderPriority Priority,
    DateTime? ScheduledStart,
    DateTime? ScheduledEnd,
    Guid? AssignedSupervisorId,
    string? Notes,
    byte[] RowVersion
);

public record ChangeWorkOrderStatusDto(
    WorkOrderStatus NewStatus,
    string? Reason,
    byte[] RowVersion
);

// ── Stage operations ─────────────────────────────────────────────────────

public record StartStageDto(
    Guid? AssignedWorkerId,
    decimal? WeightInGrams,
    byte[] RowVersion
);

public record CompleteStageDto(
    decimal? WeightOutGrams,
    decimal ActualHours,
    string? Notes,
    byte[] RowVersion
);

public record SkipStageDto(
    string Reason,
    byte[] RowVersion
);

public record FailStageDto(
    string FailureReason,
    decimal? WeightOutGrams,
    byte[] RowVersion
);

// ── Response ─────────────────────────────────────────────────────────────

public record WorkOrderStageResponseDto(
    Guid Id,
    int SequenceNumber,
    string Stage,
    string Status,
    Guid? AssignedWorkerId,
    string? AssignedWorkerName,
    decimal EstimatedHours,
    decimal? ActualHours,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    decimal? WeightInGrams,
    decimal? WeightOutGrams,
    decimal? LossGrams,
    string? Notes,
    string? FailureReason,
    byte[] RowVersion
);

public record WorkOrderResponseDto(
    Guid Id,
    string WorkOrderNumber,
    Guid? SalesOrderId,
    string? SalesOrderNumber,
    string? DesignCode,
    string Description,
    int Quantity,
    string Status,
    string Priority,
    DateTime? ScheduledStart,
    DateTime? ScheduledEnd,
    DateTime? ActualStart,
    DateTime? ActualEnd,
    DateTime? ReleasedAt,
    DateTime? CompletedAt,
    DateTime? CancelledAt,
    string? CancellationReason,
    Guid? AssignedSupervisorId,
    string? AssignedSupervisorName,
    string? Notes,
    List<WorkOrderStageResponseDto> Stages,
    byte[] RowVersion,
    DateTime CreatedAt
);
