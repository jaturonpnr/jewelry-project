using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.Suppliers.DTOs;
using JewelryFactory.Application.Features.Suppliers.Mappings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Suppliers.Queries.GetSuppliers;

public class GetSuppliersQuery : PagedRequest, IRequest<PagedResult<SupplierResponseDto>>;

public class GetSuppliersHandler(IApplicationDbContext db)
    : IRequestHandler<GetSuppliersQuery, PagedResult<SupplierResponseDto>>
{
    public async Task<PagedResult<SupplierResponseDto>> Handle(GetSuppliersQuery query, CancellationToken ct)
    {
        var q = db.Suppliers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(x =>
                EF.Functions.Like(x.CompanyName, $"%{s}%") ||
                EF.Functions.Like(x.Code, $"%{s}%"));
        }

        q = (query.Sort?.ToLowerInvariant(), query.Order?.ToLowerInvariant()) switch
        {
            ("code", "desc") => q.OrderByDescending(x => x.Code),
            ("code", _) => q.OrderBy(x => x.Code),
            ("companyname", "desc") => q.OrderByDescending(x => x.CompanyName),
            ("createdat", "desc") => q.OrderByDescending(x => x.CreatedAt),
            _ => q.OrderBy(x => x.CompanyName)
        };

        var paged = await q.ToPagedResultAsync(query.Page, query.PageSize, ct);
        return new PagedResult<SupplierResponseDto>
        {
            Items = paged.Items.Select(x => x.ToResponse()).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }
}
