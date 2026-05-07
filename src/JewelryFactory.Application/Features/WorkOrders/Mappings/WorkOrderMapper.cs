using JewelryFactory.Application.Features.WorkOrders.DTOs;
using JewelryFactory.Domain.Entities;

namespace JewelryFactory.Application.Features.WorkOrders.Mappings;

internal static class WorkOrderMapper
{
    public static WorkOrderResponseDto ToResponse(this WorkOrder w) => new(
        w.Id,
        w.WorkOrderNumber,
        w.SalesOrderId,
        w.SalesOrder?.OrderNumber,
        w.DesignCode,
        w.Description,
        w.Quantity,
        w.Status.ToString(),
        w.Priority.ToString(),
        w.ScheduledStart,
        w.ScheduledEnd,
        w.ActualStart,
        w.ActualEnd,
        w.ReleasedAt,
        w.CompletedAt,
        w.CancelledAt,
        w.CancellationReason,
        w.AssignedSupervisorId,
        w.AssignedSupervisor?.FullName,
        w.Notes,
        w.Stages.OrderBy(s => s.SequenceNumber).Select(s => s.ToResponse()).ToList(),
        w.RowVersion,
        w.CreatedAt
    );

    public static WorkOrderStageResponseDto ToResponse(this WorkOrderStage s) => new(
        s.Id,
        s.SequenceNumber,
        s.Stage.ToString(),
        s.Status.ToString(),
        s.AssignedWorkerId,
        s.AssignedWorker?.FullName,
        s.EstimatedHours,
        s.ActualHours,
        s.StartedAt,
        s.CompletedAt,
        s.WeightInGrams,
        s.WeightOutGrams,
        s.LossGrams,
        s.Notes,
        s.FailureReason,
        s.RowVersion
    );
}
