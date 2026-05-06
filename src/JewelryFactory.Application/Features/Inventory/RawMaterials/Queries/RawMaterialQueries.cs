using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.Inventory.RawMaterials.DTOs;
using JewelryFactory.Application.Features.Inventory.RawMaterials.Mappings;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Inventory.RawMaterials.Queries;

public record GetRawMaterialByIdQuery(Guid Id) : IRequest<RawMaterialResponseDto>;

public class GetRawMaterialsQuery : PagedRequest, IRequest<PagedResult<RawMaterialResponseDto>>
{
    public Guid? MaterialTypeId { get; set; }
    public string? Location { get; set; }
}

public class GetRawMaterialByIdHandler(IApplicationDbContext db)
    : IRequestHandler<GetRawMaterialByIdQuery, RawMaterialResponseDto>
{
    public async Task<RawMaterialResponseDto> Handle(GetRawMaterialByIdQuery query, CancellationToken ct)
    {
        var item = await db.RawMaterialItems
            .AsNoTracking()
            .Include(x => x.MaterialType)
            .Include(x => x.Supplier)
            .FirstOrDefaultAsync(x => x.Id == query.Id, ct)
            ?? throw new NotFoundException(nameof(RawMaterialItem), query.Id);

        return item.ToResponse();
    }
}

public class GetRawMaterialsHandler(IApplicationDbContext db)
    : IRequestHandler<GetRawMaterialsQuery, PagedResult<RawMaterialResponseDto>>
{
    public async Task<PagedResult<RawMaterialResponseDto>> Handle(GetRawMaterialsQuery query, CancellationToken ct)
    {
        var q = db.RawMaterialItems.AsNoTracking()
            .Include(x => x.MaterialType)
            .Include(x => x.Supplier)
            .AsQueryable();

        if (query.MaterialTypeId.HasValue)
            q = q.Where(x => x.MaterialTypeId == query.MaterialTypeId.Value);

        if (!string.IsNullOrWhiteSpace(query.Location))
            q = q.Where(x => x.Location == query.Location.Trim().ToUpper());

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(x =>
                EF.Functions.Like(x.LotNumber, $"%{s}%") ||
                EF.Functions.Like(x.MaterialType.Code, $"%{s}%") ||
                EF.Functions.Like(x.MaterialType.Name, $"%{s}%"));
        }

        q = (query.Sort?.ToLowerInvariant(), query.Order?.ToLowerInvariant()) switch
        {
            ("quantity", "desc") => q.OrderByDescending(x => x.Quantity),
            ("quantity", _) => q.OrderBy(x => x.Quantity),
            ("receiveddate", "desc") => q.OrderByDescending(x => x.ReceivedDate),
            ("receiveddate", _) => q.OrderBy(x => x.ReceivedDate),
            _ => q.OrderByDescending(x => x.CreatedAt)
        };

        var paged = await q.ToPagedResultAsync(query.Page, query.PageSize, ct);
        return new PagedResult<RawMaterialResponseDto>
        {
            Items = paged.Items.Select(x => x.ToResponse()).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }
}
