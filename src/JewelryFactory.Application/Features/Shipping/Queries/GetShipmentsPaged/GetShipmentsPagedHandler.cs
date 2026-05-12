using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.Shipping.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Shipping.Queries.GetShipmentsPaged;

public class GetShipmentsPagedHandler(IApplicationDbContext db)
    : IRequestHandler<GetShipmentsPagedQuery, PagedResult<ShipmentSummary>>
{
    public async Task<PagedResult<ShipmentSummary>> Handle(
        GetShipmentsPagedQuery query, CancellationToken ct)
    {
        var q = db.Shipments.AsNoTracking()
            .Where(s => !s.IsDeleted)
            .Include(s => s.SalesOrder)
            .Include(s => s.Items)
            .AsQueryable();

        if (query.Status.HasValue)
            q = q.Where(s => s.Status == query.Status.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(s => s.ShipmentNumber.ToLower().Contains(term) ||
                              (s.TrackingNumber != null && s.TrackingNumber.ToLower().Contains(term)));
        }

        var totalCount = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(s => s.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(s => new ShipmentSummary(
                s.Id, s.ShipmentNumber,
                s.SalesOrder != null ? s.SalesOrder.OrderNumber : null,
                s.Status, s.ShipDate, s.Carrier, s.TrackingNumber,
                s.Items.Count, s.TotalWeightGrams, s.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<ShipmentSummary>
        {
            Items = items, TotalCount = totalCount, Page = query.Page, PageSize = query.PageSize,
        };
    }
}
