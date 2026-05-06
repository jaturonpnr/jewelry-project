using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.Inventory.StoneItems.DTOs;
using JewelryFactory.Application.Features.Inventory.StoneItems.Mappings;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Enums;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Inventory.StoneItems.Queries;

public record GetStoneItemByIdQuery(Guid Id) : IRequest<StoneItemResponseDto>;

public class GetStoneItemsQuery : PagedRequest, IRequest<PagedResult<StoneItemResponseDto>>
{
    public StockStatus? Status { get; set; }
    public Guid? MaterialTypeId { get; set; }
}

public class GetStoneItemByIdHandler(IApplicationDbContext db)
    : IRequestHandler<GetStoneItemByIdQuery, StoneItemResponseDto>
{
    public async Task<StoneItemResponseDto> Handle(GetStoneItemByIdQuery query, CancellationToken ct)
    {
        var item = await db.StoneItems.AsNoTracking()
            .Include(x => x.MaterialType).Include(x => x.Supplier)
            .FirstOrDefaultAsync(x => x.Id == query.Id, ct)
            ?? throw new NotFoundException(nameof(StoneItem), query.Id);
        return item.ToResponse();
    }
}

public class GetStoneItemsHandler(IApplicationDbContext db)
    : IRequestHandler<GetStoneItemsQuery, PagedResult<StoneItemResponseDto>>
{
    public async Task<PagedResult<StoneItemResponseDto>> Handle(GetStoneItemsQuery query, CancellationToken ct)
    {
        var q = db.StoneItems.AsNoTracking()
            .Include(x => x.MaterialType).Include(x => x.Supplier)
            .AsQueryable();

        if (query.Status.HasValue) q = q.Where(x => x.Status == query.Status.Value);
        if (query.MaterialTypeId.HasValue) q = q.Where(x => x.MaterialTypeId == query.MaterialTypeId.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(x =>
                EF.Functions.Like(x.ItemCode, $"%{s}%") ||
                (x.CertificateNumber != null && EF.Functions.Like(x.CertificateNumber, $"%{s}%")));
        }

        q = (query.Sort?.ToLowerInvariant(), query.Order?.ToLowerInvariant()) switch
        {
            ("carat", "desc") => q.OrderByDescending(x => x.CaratWeight),
            ("carat", _) => q.OrderBy(x => x.CaratWeight),
            ("itemcode", _) => q.OrderBy(x => x.ItemCode),
            _ => q.OrderByDescending(x => x.CreatedAt)
        };

        var paged = await q.ToPagedResultAsync(query.Page, query.PageSize, ct);
        return new PagedResult<StoneItemResponseDto>
        {
            Items = paged.Items.Select(x => x.ToResponse()).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }
}
