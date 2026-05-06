using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.Customers.DTOs;
using JewelryFactory.Application.Features.Customers.Mappings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Customers.Queries.GetCustomers;

public class GetCustomersHandler(IApplicationDbContext db)
    : IRequestHandler<GetCustomersQuery, PagedResult<CustomerResponseDto>>
{
    public async Task<PagedResult<CustomerResponseDto>> Handle(GetCustomersQuery query, CancellationToken ct)
    {
        var q = db.Customers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(c =>
                EF.Functions.Like(c.CompanyName, $"%{s}%") ||
                EF.Functions.Like(c.Code, $"%{s}%") ||
                (c.ContactPerson != null && EF.Functions.Like(c.ContactPerson, $"%{s}%")));
        }

        // Default sort: CompanyName asc
        q = (query.Sort?.ToLowerInvariant(), query.Order?.ToLowerInvariant()) switch
        {
            ("code", "desc") => q.OrderByDescending(c => c.Code),
            ("code", _) => q.OrderBy(c => c.Code),
            ("companyname", "desc") => q.OrderByDescending(c => c.CompanyName),
            ("createdat", "desc") => q.OrderByDescending(c => c.CreatedAt),
            ("createdat", _) => q.OrderBy(c => c.CreatedAt),
            _ => q.OrderBy(c => c.CompanyName)
        };

        var paged = await q.ToPagedResultAsync(query.Page, query.PageSize, ct);

        return new PagedResult<CustomerResponseDto>
        {
            Items = paged.Items.Select(c => c.ToResponse()).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }
}
