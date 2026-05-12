using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.QualityControl.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.QualityControl.Queries.GetQcInspectionsPaged;

public class GetQcInspectionsPagedHandler(IApplicationDbContext db)
    : IRequestHandler<GetQcInspectionsPagedQuery, PagedResult<QcInspectionSummary>>
{
    public async Task<PagedResult<QcInspectionSummary>> Handle(
        GetQcInspectionsPagedQuery query, CancellationToken ct)
    {
        var q = db.QcInspections
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .Include(x => x.WorkOrder)
            .Include(x => x.RawMaterialItem)
            .Include(x => x.Defects)
            .AsQueryable();

        if (query.InspectionType.HasValue)
            q = q.Where(x => x.InspectionType == query.InspectionType.Value);

        if (query.Result.HasValue)
            q = q.Where(x => x.Result == query.Result.Value);

        if (query.WorkOrderId.HasValue)
            q = q.Where(x => x.WorkOrderId == query.WorkOrderId.Value);

        if (query.FromDate.HasValue)
            q = q.Where(x => x.InspectionDate >= query.FromDate.Value.ToUniversalTime());

        if (query.ToDate.HasValue)
            q = q.Where(x => x.InspectionDate <= query.ToDate.Value.ToUniversalTime());

        var totalCount = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(x => x.InspectionDate)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new QcInspectionSummary(
                x.Id,
                x.InspectionType,
                x.InspectionDate,
                x.WorkOrder != null ? x.WorkOrder.WorkOrderNumber : null,
                x.RawMaterialItem != null ? x.RawMaterialItem.LotNumber : null,
                x.InspectorName,
                x.Result,
                x.Defects.Count,
                x.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<QcInspectionSummary>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }
}
