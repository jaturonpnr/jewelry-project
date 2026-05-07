using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.Reports.DTOs;
using JewelryFactory.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Reports.Queries;

public record GetInventorySummaryReportQuery() : IRequest<InventorySummaryReportDto>;

public class GetInventorySummaryReportHandler(IApplicationDbContext db)
    : IRequestHandler<GetInventorySummaryReportQuery, InventorySummaryReportDto>
{
    public async Task<InventorySummaryReportDto> Handle(GetInventorySummaryReportQuery query, CancellationToken ct)
    {
        // Pull raw groupings (enum keys) — EF can't translate Enum.ToString() inside GroupBy projection,
        // so we map to DTOs in memory after the round-trip.
        var rawByCategoryRaw = await db.RawMaterialItems.AsNoTracking()
            .Where(r => r.Status == StockStatus.InStock)
            .GroupBy(r => r.MaterialType.Category)
            .Select(g => new
            {
                Category = g.Key,
                LotCount = g.Count(),
                TotalQuantity = g.Sum(r => r.Quantity),
            })
            .ToListAsync(ct);

        var rawByCategory = rawByCategoryRaw
            .Select(x => new RawMaterialByCategoryDto(x.Category.ToString(), x.LotCount, x.TotalQuantity))
            .OrderBy(x => x.Category)
            .ToList();

        var rawByMaterialRaw = await db.RawMaterialItems.AsNoTracking()
            .Where(r => r.Status == StockStatus.InStock)
            .GroupBy(r => new { r.MaterialType.Code, r.MaterialType.Name, r.MaterialType.Unit })
            .Select(g => new
            {
                g.Key.Code,
                g.Key.Name,
                Unit = g.Key.Unit,
                LotCount = g.Count(),
                TotalQuantity = g.Sum(r => r.Quantity),
                TotalPureWeight = g.Sum(r => r.PureWeight ?? 0),
            })
            .ToListAsync(ct);

        var rawByMaterial = rawByMaterialRaw
            .Select(x => new RawMaterialByMaterialDto(
                x.Code, x.Name, x.Unit.ToString(),
                x.LotCount, x.TotalQuantity,
                x.TotalPureWeight == 0 ? null : x.TotalPureWeight))
            .OrderBy(x => x.MaterialCode)
            .ToList();

        var rawByStatus = (await db.RawMaterialItems.AsNoTracking()
                .GroupBy(r => r.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(ct))
            .Select(x => new StockByStatusDto(x.Status.ToString(), x.Count))
            .OrderBy(x => x.Status)
            .ToList();

        var stoneItemsByStatus = (await db.StoneItems.AsNoTracking()
                .GroupBy(s => s.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(ct))
            .Select(x => new StockByStatusDto(x.Status.ToString(), x.Count))
            .OrderBy(x => x.Status)
            .ToList();

        var stoneParcelsByStatus = (await db.StoneParcels.AsNoTracking()
                .GroupBy(p => p.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(ct))
            .Select(x => new StockByStatusDto(x.Status.ToString(), x.Count))
            .OrderBy(x => x.Status)
            .ToList();

        // Totals
        var totalPureGold = await db.RawMaterialItems.AsNoTracking()
            .Where(r => r.Status == StockStatus.InStock
                     && r.MaterialType.Category == MaterialCategory.Metal)
            .SumAsync(r => r.PureWeight ?? 0, ct);

        var rawLotCount = await db.RawMaterialItems.AsNoTracking()
            .CountAsync(r => r.Status == StockStatus.InStock, ct);
        var stoneItemCount = await db.StoneItems.AsNoTracking()
            .CountAsync(s => s.Status == StockStatus.InStock, ct);
        var stoneParcelCount = await db.StoneParcels.AsNoTracking()
            .CountAsync(p => p.Status == StockStatus.InStock, ct);

        var stoneItemCarat = await db.StoneItems.AsNoTracking()
            .Where(s => s.Status == StockStatus.InStock)
            .SumAsync(s => s.CaratWeight, ct);
        var stoneParcelCarat = await db.StoneParcels.AsNoTracking()
            .Where(p => p.Status == StockStatus.InStock)
            .SumAsync(p => p.TotalCarat, ct);
        var stoneParcelStones = await db.StoneParcels.AsNoTracking()
            .Where(p => p.Status == StockStatus.InStock)
            .SumAsync(p => p.StoneCount, ct);

        var totals = new InventoryTotalsDto(
            RawMaterialLotCount: rawLotCount,
            StoneItemCount: stoneItemCount,
            StoneParcelCount: stoneParcelCount,
            TotalPureGoldGrams: totalPureGold,
            TotalStoneCarat: stoneItemCarat + stoneParcelCarat,
            TotalStoneCount: stoneItemCount + stoneParcelStones);

        return new InventorySummaryReportDto(
            totals,
            rawByCategory,
            rawByMaterial,
            rawByStatus,
            stoneItemsByStatus,
            stoneParcelsByStatus);
    }
}
