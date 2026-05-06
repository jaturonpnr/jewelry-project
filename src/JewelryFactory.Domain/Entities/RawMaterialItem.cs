using JewelryFactory.Domain.Common;
using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Domain.Entities;

/// <summary>
/// Raw material in stock — gold, silver, findings, consumables.
/// One row per (MaterialType + Lot + Location) combination.
/// Quantity is in MaterialType.Unit (gram/piece/liter).
/// </summary>
public class RawMaterialItem : ConcurrentEntity
{
    public required Guid MaterialTypeId { get; set; }
    public MaterialType MaterialType { get; set; } = null!;

    public required string LotNumber { get; set; }       // batch identifier from supplier
    public required string Location { get; set; }        // warehouse / safe slot

    /// <summary>
    /// Gross quantity in the unit defined by MaterialType.Unit.
    /// For metals: grams (4 decimal places per CLAUDE.md §10.1).
    /// </summary>
    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }                // cost per unit
    public Currency CostCurrency { get; set; } = Currency.USD;

    public Guid? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public DateTime ReceivedDate { get; set; }
    public StockStatus Status { get; set; } = StockStatus.InStock;
    public string? Notes { get; set; }

    /// <summary>
    /// For metals only — pure metal weight (Quantity × MaterialType.PurityFraction).
    /// CLAUDE.md §10.3.
    /// </summary>
    public decimal? PureWeight { get; set; }
}
