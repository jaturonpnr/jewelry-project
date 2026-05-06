using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.MaterialTypes.DTOs;
using JewelryFactory.Application.Features.MaterialTypes.Mappings;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.MaterialTypes.Queries;

public record GetMaterialTypeByIdQuery(Guid Id) : IRequest<MaterialTypeResponseDto>;
public class GetMaterialTypesQuery : PagedRequest, IRequest<PagedResult<MaterialTypeResponseDto>>;

public class GetMaterialTypeByIdHandler(IApplicationDbContext db)
    : IRequestHandler<GetMaterialTypeByIdQuery, MaterialTypeResponseDto>
{
    public async Task<MaterialTypeResponseDto> Handle(GetMaterialTypeByIdQuery query, CancellationToken ct)
    {
        var entity = await db.MaterialTypes.AsNoTracking().FirstOrDefaultAsync(m => m.Id == query.Id, ct)
            ?? throw new NotFoundException(nameof(MaterialType), query.Id);
        return entity.ToResponse();
    }
}

public class GetMaterialTypesHandler(IApplicationDbContext db)
    : IRequestHandler<GetMaterialTypesQuery, PagedResult<MaterialTypeResponseDto>>
{
    public async Task<PagedResult<MaterialTypeResponseDto>> Handle(GetMaterialTypesQuery query, CancellationToken ct)
    {
        var q = db.MaterialTypes.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(m =>
                EF.Functions.Like(m.Code, $"%{s}%") ||
                EF.Functions.Like(m.Name, $"%{s}%"));
        }

        q = (query.Sort?.ToLowerInvariant(), query.Order?.ToLowerInvariant()) switch
        {
            ("name", "desc") => q.OrderByDescending(m => m.Name),
            ("name", _) => q.OrderBy(m => m.Name),
            ("category", "desc") => q.OrderByDescending(m => m.Category),
            ("category", _) => q.OrderBy(m => m.Category),
            _ => q.OrderBy(m => m.Code)
        };

        var paged = await q.ToPagedResultAsync(query.Page, query.PageSize, ct);
        return new PagedResult<MaterialTypeResponseDto>
        {
            Items = paged.Items.Select(m => m.ToResponse()).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }
}
