using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.Inventory.StockMovements.DTOs;
using JewelryFactory.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Inventory.StockMovements.Queries;

public class GetStockMovementsQuery : PagedRequest, IRequest<PagedResult<StockMovementResponseDto>>
{
    public InventoryItemType? ItemType { get; set; }
    public Guid? ItemId { get; set; }
    public StockMovementType? MovementType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class GetStockMovementsHandler(IApplicationDbContext db)
    : IRequestHandler<GetStockMovementsQuery, PagedResult<StockMovementResponseDto>>
{
    public async Task<PagedResult<StockMovementResponseDto>> Handle(GetStockMovementsQuery query, CancellationToken ct)
    {
        var q = db.StockMovements.AsNoTracking().AsQueryable();

        if (query.ItemType.HasValue) q = q.Where(x => x.ItemType == query.ItemType.Value);
        if (query.ItemId.HasValue) q = q.Where(x => x.ItemId == query.ItemId.Value);
        if (query.MovementType.HasValue) q = q.Where(x => x.MovementType == query.MovementType.Value);
        if (query.FromDate.HasValue) q = q.Where(x => x.PerformedAt >= query.FromDate.Value);
        if (query.ToDate.HasValue) q = q.Where(x => x.PerformedAt <= query.ToDate.Value);

        q = q.OrderByDescending(x => x.PerformedAt);

        var paged = await q.ToPagedResultAsync(query.Page, query.PageSize, ct);

        return new PagedResult<StockMovementResponseDto>
        {
            Items = paged.Items.Select(m => new StockMovementResponseDto(
                m.Id,
                m.ItemType.ToString(),
                m.ItemId,
                m.MovementType.ToString(),
                m.QuantityDelta,
                m.QuantityAfter,
                m.ReferenceType,
                m.ReferenceId,
                m.Reason,
                m.Notes,
                m.PerformedBy,
                m.PerformedAt
            )).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }
}
