using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.SalesOrders.DTOs;
using JewelryFactory.Application.Features.SalesOrders.Mappings;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Enums;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.SalesOrders.Queries;

public record GetSalesOrderByIdQuery(Guid Id) : IRequest<SalesOrderResponseDto>;

public class GetSalesOrdersQuery : PagedRequest, IRequest<PagedResult<SalesOrderResponseDto>>
{
    public SalesOrderStatus? Status { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class GetSalesOrderByIdHandler(IApplicationDbContext db)
    : IRequestHandler<GetSalesOrderByIdQuery, SalesOrderResponseDto>
{
    public async Task<SalesOrderResponseDto> Handle(GetSalesOrderByIdQuery query, CancellationToken ct)
    {
        var o = await db.SalesOrders.AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == query.Id, ct)
            ?? throw new NotFoundException(nameof(SalesOrder), query.Id);
        return o.ToResponse();
    }
}

public class GetSalesOrdersHandler(IApplicationDbContext db)
    : IRequestHandler<GetSalesOrdersQuery, PagedResult<SalesOrderResponseDto>>
{
    public async Task<PagedResult<SalesOrderResponseDto>> Handle(GetSalesOrdersQuery query, CancellationToken ct)
    {
        var q = db.SalesOrders.AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Items)
            .AsQueryable();

        if (query.Status.HasValue) q = q.Where(x => x.Status == query.Status.Value);
        if (query.CustomerId.HasValue) q = q.Where(x => x.CustomerId == query.CustomerId.Value);
        if (query.FromDate.HasValue) q = q.Where(x => x.OrderDate >= query.FromDate.Value);
        if (query.ToDate.HasValue) q = q.Where(x => x.OrderDate <= query.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(x =>
                EF.Functions.Like(x.OrderNumber, $"%{s}%") ||
                EF.Functions.Like(x.Customer.CompanyName, $"%{s}%"));
        }

        q = (query.Sort?.ToLowerInvariant(), query.Order?.ToLowerInvariant()) switch
        {
            ("ordernumber", "desc") => q.OrderByDescending(x => x.OrderNumber),
            ("ordernumber", _) => q.OrderBy(x => x.OrderNumber),
            ("orderdate", _) => q.OrderBy(x => x.OrderDate),
            ("totalamount", "desc") => q.OrderByDescending(x => x.TotalAmount),
            _ => q.OrderByDescending(x => x.OrderDate)
        };

        var paged = await q.ToPagedResultAsync(query.Page, query.PageSize, ct);
        return new PagedResult<SalesOrderResponseDto>
        {
            Items = paged.Items.Select(o => o.ToResponse()).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }
}
