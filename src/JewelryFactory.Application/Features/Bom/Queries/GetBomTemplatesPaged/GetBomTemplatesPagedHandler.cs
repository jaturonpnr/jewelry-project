using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.Bom.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Bom.Queries.GetBomTemplatesPaged;

public class GetBomTemplatesPagedHandler(IApplicationDbContext db)
    : IRequestHandler<GetBomTemplatesPagedQuery, PagedResult<BomTemplateSummaryResponse>>
{
    public async Task<PagedResult<BomTemplateSummaryResponse>> Handle(
        GetBomTemplatesPagedQuery query, CancellationToken ct)
    {
        var q = db.BomTemplates
            .AsNoTracking()
            .Where(b => !b.IsDeleted)
            .Include(b => b.MaterialLines)
            .Include(b => b.StoneLines)
            .Include(b => b.LaborLines)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(b => b.DesignCode.ToLower().Contains(term) ||
                              b.DesignName.ToLower().Contains(term));
        }

        if (query.IsActive.HasValue)
            q = q.Where(b => b.IsActive == query.IsActive.Value);

        q = q.OrderBy(b => b.DesignCode);

        var boms = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        var totalCount = await db.BomTemplates
            .AsNoTracking()
            .CountAsync(b => !b.IsDeleted, ct);

        var items = boms.Select(BomMapper.ToSummary).ToList();

        return new PagedResult<BomTemplateSummaryResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }
}
