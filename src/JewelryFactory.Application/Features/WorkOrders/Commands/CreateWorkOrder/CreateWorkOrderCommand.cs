using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.WorkOrders.DTOs;
using JewelryFactory.Application.Features.WorkOrders.Mappings;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Enums;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.WorkOrders.Commands.CreateWorkOrder;

public record CreateWorkOrderCommand(CreateWorkOrderDto Request) : IRequest<WorkOrderResponseDto>;

public class CreateWorkOrderHandler(IApplicationDbContext db)
    : IRequestHandler<CreateWorkOrderCommand, WorkOrderResponseDto>
{
    public async Task<WorkOrderResponseDto> Handle(CreateWorkOrderCommand command, CancellationToken ct)
    {
        var dto = command.Request;
        var woNumber = dto.WorkOrderNumber.Trim().ToUpperInvariant();

        if (await db.WorkOrders.AnyAsync(w => w.WorkOrderNumber == woNumber, ct))
            throw new BusinessRuleException($"Work order number '{woNumber}' is already in use.");

        // Optional sales order link
        if (dto.SalesOrderId.HasValue)
        {
            var soExists = await db.SalesOrders.AnyAsync(s => s.Id == dto.SalesOrderId.Value, ct);
            if (!soExists)
                throw new NotFoundException(nameof(SalesOrder), dto.SalesOrderId.Value);
        }

        // Optional supervisor
        if (dto.AssignedSupervisorId.HasValue)
        {
            var workerExists = await db.Workers.AnyAsync(w => w.Id == dto.AssignedSupervisorId.Value, ct);
            if (!workerExists)
                throw new NotFoundException(nameof(Worker), dto.AssignedSupervisorId.Value);
        }

        var workOrder = new WorkOrder
        {
            WorkOrderNumber = woNumber,
            SalesOrderId = dto.SalesOrderId,
            DesignCode = dto.DesignCode?.Trim(),
            Description = dto.Description.Trim(),
            Quantity = dto.Quantity,
            Priority = dto.Priority,
            ScheduledStart = dto.ScheduledStart,
            ScheduledEnd = dto.ScheduledEnd,
            AssignedSupervisorId = dto.AssignedSupervisorId,
            Notes = dto.Notes?.Trim(),
            Status = WorkOrderStatus.Draft,
        };

        // Auto-create all 9 stages in canonical order
        var estimates = (dto.StageEstimates ?? [])
            .GroupBy(e => e.Stage)
            .ToDictionary(g => g.Key, g => g.First());

        var seq = 1;
        foreach (var stage in Enum.GetValues<ProductionStage>())
        {
            var est = estimates.GetValueOrDefault(stage);
            workOrder.Stages.Add(new WorkOrderStage
            {
                Stage = stage,
                SequenceNumber = seq++,
                Status = WorkOrderStageStatus.Pending,
                EstimatedHours = est?.EstimatedHours ?? 0m,
                AssignedWorkerId = est?.AssignedWorkerId,
            });
        }

        db.WorkOrders.Add(workOrder);
        await db.SaveChangesAsync(ct);

        var saved = await db.WorkOrders
            .Include(w => w.SalesOrder)
            .Include(w => w.AssignedSupervisor)
            .Include(w => w.Stages).ThenInclude(s => s.AssignedWorker)
            .FirstAsync(w => w.Id == workOrder.Id, ct);

        return saved.ToResponse();
    }
}
