using JewelryFactory.Domain.Common;
using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Domain.Entities;

/// <summary>
/// Production work order. Drives the 9-stage workflow.
/// Phase 1 - Module 5.
/// </summary>
public class WorkOrder : ConcurrentEntity
{
    public required string WorkOrderNumber { get; set; }   // unique, e.g. WO-2026-0001

    /// <summary>
    /// Optional link to the sales order this WO fulfills.
    /// Null = stock production (build to inventory).
    /// </summary>
    public Guid? SalesOrderId { get; set; }
    public SalesOrder? SalesOrder { get; set; }

    public string? DesignCode { get; set; }                // future link to Design entity
    public required string Description { get; set; }
    public int Quantity { get; set; }                      // pieces to produce

    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Draft;
    public WorkOrderPriority Priority { get; set; } = WorkOrderPriority.Normal;

    // Schedule
    public DateTime? ScheduledStart { get; set; }
    public DateTime? ScheduledEnd { get; set; }
    public DateTime? ActualStart { get; set; }
    public DateTime? ActualEnd { get; set; }

    // Lifecycle timestamps
    public DateTime? ReleasedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    public Guid? AssignedSupervisorId { get; set; }        // Worker
    public Worker? AssignedSupervisor { get; set; }

    public string? Notes { get; set; }

    public ICollection<WorkOrderStage> Stages { get; set; } = new List<WorkOrderStage>();
}
