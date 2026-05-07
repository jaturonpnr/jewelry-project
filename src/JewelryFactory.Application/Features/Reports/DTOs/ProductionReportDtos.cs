namespace JewelryFactory.Application.Features.Reports.DTOs;

public record ProductionLoadReportDto(
    int ActiveWorkOrders,              // Released or InProgress
    int CompletedThisMonth,
    int CancelledThisMonth,
    decimal TotalLossGramsThisMonth,   // sum of LossGrams across stages completed this month

    IReadOnlyList<WorkOrdersByStatusDto> WorkOrdersByStatus,
    IReadOnlyList<WorkOrdersByPriorityDto> WorkOrdersByPriority,
    IReadOnlyList<StagesByStageDto> StagesInProgress,
    IReadOnlyList<WorkerLoadDto> WorkerLoad
);

public record WorkOrdersByStatusDto(
    string Status,
    int Count
);

public record WorkOrdersByPriorityDto(
    string Priority,
    int Count
);

public record StagesByStageDto(
    string Stage,                      // WaxModel, Casting, ...
    int InProgressCount,
    int PendingCount,
    int CompletedCount,
    decimal? TotalLossGrams            // sum across all completed stages of this kind
);

public record WorkerLoadDto(
    Guid WorkerId,
    string EmployeeCode,
    string FullName,
    string Position,
    int InProgressStages,
    int PendingStages
);
