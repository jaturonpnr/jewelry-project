using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.WorkOrders.DTOs;
using JewelryFactory.Application.Features.WorkOrders.Mappings;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Enums;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JewelryFactory.Application.Features.WorkOrders.Commands.ChangeWorkOrderStatus;

public record ChangeWorkOrderStatusCommand(Guid Id, ChangeWorkOrderStatusDto Request)
    : IRequest<WorkOrderResponseDto>;

public class ChangeWorkOrderStatusHandler(
    IApplicationDbContext db,
    ILogger<ChangeWorkOrderStatusHandler> logger
) : IRequestHandler<ChangeWorkOrderStatusCommand, WorkOrderResponseDto>
{
    public async Task<WorkOrderResponseDto> Handle(ChangeWorkOrderStatusCommand command, CancellationToken ct)
    {
        var wo = await db.WorkOrders
            .Include(w => w.SalesOrder)
            .Include(w => w.AssignedSupervisor)
            .Include(w => w.Stages).ThenInclude(s => s.AssignedWorker)
            .FirstOrDefaultAsync(w => w.Id == command.Id, ct)
            ?? throw new NotFoundException(nameof(WorkOrder), command.Id);

        var dto = command.Request;
        var oldStatus = wo.Status;
        var newStatus = dto.NewStatus;

        if (oldStatus == newStatus)
            return wo.ToResponse();

        if (!WorkOrderStatusFlow.CanTransition(oldStatus, newStatus))
            throw new BusinessRuleException($"Invalid transition: {oldStatus} → {newStatus}.");

        if (newStatus == WorkOrderStatus.Cancelled && string.IsNullOrWhiteSpace(dto.Reason))
            throw new BusinessRuleException("Cancellation reason is required.");

        // Special rule: Completing requires every non-skipped stage to be Completed.
        if (newStatus == WorkOrderStatus.Completed)
        {
            var unfinished = wo.Stages.Any(s =>
                s.Status != WorkOrderStageStatus.Completed &&
                s.Status != WorkOrderStageStatus.Skipped);
            if (unfinished)
                throw new BusinessRuleException(
                    "All stages must be Completed or Skipped before completing the work order.");
        }

        // Set lifecycle timestamps
        var now = DateTime.UtcNow;
        switch (newStatus)
        {
            case WorkOrderStatus.Released:
                wo.ReleasedAt = now;
                break;
            case WorkOrderStatus.InProgress:
                wo.ActualStart ??= now;
                break;
            case WorkOrderStatus.Completed:
                wo.CompletedAt = now;
                wo.ActualEnd = now;
                break;
            case WorkOrderStatus.Cancelled:
                wo.CancelledAt = now;
                wo.CancellationReason = dto.Reason!.Trim();
                break;
        }

        wo.Status = newStatus;
        db.Entry(wo).Property(x => x.RowVersion).OriginalValue = dto.RowVersion;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException(nameof(WorkOrder), command.Id);
        }

        logger.LogInformation("WorkOrder {WoNumber}: {From} → {To}",
            wo.WorkOrderNumber, oldStatus, newStatus);

        return wo.ToResponse();
    }
}
