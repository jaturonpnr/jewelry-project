using JewelryFactory.Domain.Common;
using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Domain.Entities;

/// <summary>
/// Parcel of small stones — for stones &lt; 0.20 carat (melee diamonds, accent stones).
/// Tracked as a batch with total carat + count (per CLAUDE.md §10.4).
/// </summary>
public class StoneParcel : ConcurrentEntity
{
    public required string ParcelCode { get; set; }           // e.g. PCL-2026-0001

    public required Guid MaterialTypeId { get; set; }
    public MaterialType MaterialType { get; set; } = null!;

    public decimal TotalCarat { get; set; }                   // total weight of parcel
    public int StoneCount { get; set; }                       // approximate count
    public decimal AverageSize { get; set; }                  // ct per stone (computed snapshot)

    public string? QualityGrade { get; set; }                 // e.g. "VS-G" general grade
    public StoneShape Shape { get; set; } = StoneShape.Round;

    public Guid? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public DateTime ReceivedDate { get; set; }
    public string Location { get; set; } = "MAIN";

    public decimal UnitCostPerCarat { get; set; }
    public Currency CostCurrency { get; set; } = Currency.USD;

    public StockStatus Status { get; set; } = StockStatus.InStock;
    public string? Notes { get; set; }
}
