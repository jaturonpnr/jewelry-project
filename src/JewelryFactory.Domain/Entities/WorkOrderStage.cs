using JewelryFactory.Domain.Common;
using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Domain.Entities;

/// <summary>
/// One stage of a Work Order. Auto-created in WorkOrder constructor for all 9 stages.
/// Per-stage weight reconciliation per CLAUDE.md §10.6.
/// </summary>
public class WorkOrderStage : ConcurrentEntity
{
    public Guid WorkOrderId { get; set; }
    public WorkOrder WorkOrder { get; set; } = null!;

    public required ProductionStage Stage { get; set; }
    public int SequenceNumber { get; set; }                // 1..9 — display order
    public WorkOrderStageStatus Status { get; set; } = WorkOrderStageStatus.Pending;

    public Guid? AssignedWorkerId { get; set; }
    public Worker? AssignedWorker { get; set; }

    // Time tracking
    public decimal EstimatedHours { get; set; }
    public decimal? ActualHours { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Weight in grams ENTERING this stage (gross weight, 4 decimals per CLAUDE.md §10.1).
    /// </summary>
    public decimal? WeightInGrams { get; set; }

    /// <summary>
    /// Weight in grams EXITING this stage (after work).
    /// </summary>
    public decimal? WeightOutGrams { get; set; }

    /// <summary>
    /// Computed loss = WeightIn - WeightOut. Positive = loss; negative = unexpected gain (data error).
    /// </summary>
    public decimal? LossGrams => WeightInGrams.HasValue && WeightOutGrams.HasValue
        ? WeightInGrams.Value - WeightOutGrams.Value
        : null;

    public string? Notes { get; set; }
    public string? FailureReason { get; set; }             // when Status = Failed
}
