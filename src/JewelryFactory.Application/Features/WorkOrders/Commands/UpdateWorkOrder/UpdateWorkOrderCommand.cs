using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.WorkOrders.DTOs;
using JewelryFactory.Application.Features.WorkOrders.Mappings;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Enums;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.WorkOrders.Commands.UpdateWorkOrder;

/// <summary>
/// Update header fields. Allowed only while in Draft status — once released,
/// the production team owns it and headers shouldn't change underneath them.
/// </summary>
public record UpdateWorkOrderCommand(Guid Id, UpdateWorkOrderDto Request) : IRequest<WorkOrderResponseDto>;

public class UpdateWorkOrderHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateWorkOrderCommand, WorkOrderResponseDto>
{
    public async Task<WorkOrderResponseDto> Handle(UpdateWorkOrderCommand command, CancellationToken ct)
    {
        var wo = await db.WorkOrders
            .Include(w => w.SalesOrder)
            .Include(w => w.AssignedSupervisor)
            .Include(w => w.Stages).ThenInclude(s => s.AssignedWorker)
            .FirstOrDefaultAsync(w => w.Id == command.Id, ct)
            ?? throw new NotFoundException(nameof(WorkOrder), command.Id);

        if (wo.Status != WorkOrderStatus.Draft)
            throw new BusinessRuleException(
                $"Work order can only be edited while Draft (current: {wo.Status}).");

        var dto = command.Request;
        wo.DesignCode = dto.DesignCode?.Trim();
        wo.Description = dto.Description.Trim();
        wo.Quantity = dto.Quantity;
        wo.Priority = dto.Priority;
        wo.ScheduledStart = dto.ScheduledStart;
        wo.ScheduledEnd = dto.ScheduledEnd;
        wo.AssignedSupervisorId = dto.AssignedSupervisorId;
        wo.Notes = dto.Notes?.Trim();

        db.Entry(wo).Property(x => x.RowVersion).OriginalValue = dto.RowVersion;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException(nameof(WorkOrder), command.Id);
        }

        return wo.ToResponse();
    }
}
