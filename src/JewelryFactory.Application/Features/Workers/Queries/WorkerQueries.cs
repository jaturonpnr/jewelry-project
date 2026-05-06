using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.Workers.DTOs;
using JewelryFactory.Application.Features.Workers.Mappings;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Workers.Queries;

public record GetWorkerByIdQuery(Guid Id) : IRequest<WorkerResponseDto>;
public class GetWorkersQuery : PagedRequest, IRequest<PagedResult<WorkerResponseDto>>;

public class GetWorkerByIdHandler(IApplicationDbContext db)
    : IRequestHandler<GetWorkerByIdQuery, WorkerResponseDto>
{
    public async Task<WorkerResponseDto> Handle(GetWorkerByIdQuery query, CancellationToken ct)
    {
        var worker = await db.Workers.AsNoTracking().FirstOrDefaultAsync(w => w.Id == query.Id, ct)
            ?? throw new NotFoundException(nameof(Worker), query.Id);
        return worker.ToResponse();
    }
}

public class GetWorkersHandler(IApplicationDbContext db)
    : IRequestHandler<GetWorkersQuery, PagedResult<WorkerResponseDto>>
{
    public async Task<PagedResult<WorkerResponseDto>> Handle(GetWorkersQuery query, CancellationToken ct)
    {
        var q = db.Workers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(w =>
                EF.Functions.Like(w.FullName, $"%{s}%") ||
                EF.Functions.Like(w.EmployeeCode, $"%{s}%"));
        }

        q = (query.Sort?.ToLowerInvariant(), query.Order?.ToLowerInvariant()) switch
        {
            ("employeecode", "desc") => q.OrderByDescending(w => w.EmployeeCode),
            ("employeecode", _) => q.OrderBy(w => w.EmployeeCode),
            ("fullname", "desc") => q.OrderByDescending(w => w.FullName),
            ("hireddate", "desc") => q.OrderByDescending(w => w.HiredDate),
            _ => q.OrderBy(w => w.FullName)
        };

        var paged = await q.ToPagedResultAsync(query.Page, query.PageSize, ct);
        return new PagedResult<WorkerResponseDto>
        {
            Items = paged.Items.Select(w => w.ToResponse()).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }
}
