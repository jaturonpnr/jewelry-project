using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.WorkOrders.DTOs;
using JewelryFactory.Application.Features.WorkOrders.Mappings;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Enums;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.WorkOrders.Queries;

public record GetWorkOrderByIdQuery(Guid Id) : IRequest<WorkOrderResponseDto>;

public class GetWorkOrdersQuery : PagedRequest, IRequest<PagedResult<WorkOrderResponseDto>>
{
    public WorkOrderStatus? Status { get; set; }
    public WorkOrderPriority? Priority { get; set; }
    public Guid? SalesOrderId { get; set; }
    public Guid? AssignedSupervisorId { get; set; }
}

public class GetWorkOrderByIdHandler(IApplicationDbContext db)
    : IRequestHandler<GetWorkOrderByIdQuery, WorkOrderResponseDto>
{
    public async Task<WorkOrderResponseDto> Handle(GetWorkOrderByIdQuery query, CancellationToken ct)
    {
        var wo = await db.WorkOrders.AsNoTracking()
            .Include(w => w.SalesOrder)
            .Include(w => w.AssignedSupervisor)
            .Include(w => w.Stages).ThenInclude(s => s.AssignedWorker)
            .FirstOrDefaultAsync(w => w.Id == query.Id, ct)
            ?? throw new NotFoundException(nameof(WorkOrder), query.Id);
        return wo.ToResponse();
    }
}

public class GetWorkOrdersHandler(IApplicationDbContext db)
    : IRequestHandler<GetWorkOrdersQuery, PagedResult<WorkOrderResponseDto>>
{
    public async Task<PagedResult<WorkOrderResponseDto>> Handle(GetWorkOrdersQuery query, CancellationToken ct)
    {
        var q = db.WorkOrders.AsNoTracking()
            .Include(w => w.SalesOrder)
            .Include(w => w.AssignedSupervisor)
            .Include(w => w.Stages).ThenInclude(s => s.AssignedWorker)
            .AsQueryable();

        if (query.Status.HasValue) q = q.Where(w => w.Status == query.Status.Value);
        if (query.Priority.HasValue) q = q.Where(w => w.Priority == query.Priority.Value);
        if (query.SalesOrderId.HasValue) q = q.Where(w => w.SalesOrderId == query.SalesOrderId.Value);
        if (query.AssignedSupervisorId.HasValue) q = q.Where(w => w.AssignedSupervisorId == query.AssignedSupervisorId.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(w =>
                EF.Functions.Like(w.WorkOrderNumber, $"%{s}%") ||
                EF.Functions.Like(w.Description, $"%{s}%"));
        }

        q = (query.Sort?.ToLowerInvariant(), query.Order?.ToLowerInvariant()) switch
        {
            ("workordernumber", "desc") => q.OrderByDescending(w => w.WorkOrderNumber),
            ("workordernumber", _) => q.OrderBy(w => w.WorkOrderNumber),
            ("priority", _) => q.OrderByDescending(w => w.Priority).ThenByDescending(w => w.CreatedAt),
            ("scheduledstart", _) => q.OrderBy(w => w.ScheduledStart),
            _ => q.OrderByDescending(w => w.CreatedAt)
        };

        var paged = await q.ToPagedResultAsync(query.Page, query.PageSize, ct);
        return new PagedResult<WorkOrderResponseDto>
        {
            Items = paged.Items.Select(w => w.ToResponse()).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }
}
