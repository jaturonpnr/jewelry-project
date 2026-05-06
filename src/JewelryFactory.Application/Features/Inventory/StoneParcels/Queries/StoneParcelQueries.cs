using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.Inventory.StoneParcels.DTOs;
using JewelryFactory.Application.Features.Inventory.StoneParcels.Mappings;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Inventory.StoneParcels.Queries;

public record GetStoneParcelByIdQuery(Guid Id) : IRequest<StoneParcelResponseDto>;
public class GetStoneParcelsQuery : PagedRequest, IRequest<PagedResult<StoneParcelResponseDto>>;

public class GetStoneParcelByIdHandler(IApplicationDbContext db)
    : IRequestHandler<GetStoneParcelByIdQuery, StoneParcelResponseDto>
{
    public async Task<StoneParcelResponseDto> Handle(GetStoneParcelByIdQuery query, CancellationToken ct)
    {
        var p = await db.StoneParcels.AsNoTracking()
            .Include(x => x.MaterialType).Include(x => x.Supplier)
            .FirstOrDefaultAsync(x => x.Id == query.Id, ct)
            ?? throw new NotFoundException(nameof(StoneParcel), query.Id);
        return p.ToResponse();
    }
}

public class GetStoneParcelsHandler(IApplicationDbContext db)
    : IRequestHandler<GetStoneParcelsQuery, PagedResult<StoneParcelResponseDto>>
{
    public async Task<PagedResult<StoneParcelResponseDto>> Handle(GetStoneParcelsQuery query, CancellationToken ct)
    {
        var q = db.StoneParcels.AsNoTracking()
            .Include(x => x.MaterialType).Include(x => x.Supplier)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(x => EF.Functions.Like(x.ParcelCode, $"%{s}%"));
        }

        q = q.OrderByDescending(x => x.CreatedAt);

        var paged = await q.ToPagedResultAsync(query.Page, query.PageSize, ct);
        return new PagedResult<StoneParcelResponseDto>
        {
            Items = paged.Items.Select(x => x.ToResponse()).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }
}
