using JewelryFactory.Domain.Common;
using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Domain.Entities;

/// <summary>
/// One raw-material entry in a BOM (metals, findings, consumables).
/// Weight is per finished piece; loss is the acceptable wastage at this stage.
/// </summary>
public class BomMaterialLine : BaseEntity
{
    public Guid BomTemplateId { get; set; }
    public BomTemplate BomTemplate { get; set; } = null!;

    public MaterialCategory Category { get; set; }
    public required string MaterialDescription { get; set; }  // e.g. "18K Yellow Gold"

    /// <summary>Karat purity for metals (null for non-metals).</summary>
    public int? Karat { get; set; }

    /// <summary>
    /// Purity as decimal fraction: 18K = 0.7500, 9K = 0.3750.
    /// Null for non-precious materials.
    /// </summary>
    public decimal? PurityFraction { get; set; }

    /// <summary>Gross weight required per piece (grams).</summary>
    public decimal QuantityGrams { get; set; }

    /// <summary>Expected production loss at this stage (%).</summary>
    public decimal ExpectedLossPercent { get; set; }

    /// <summary>Unit cost per gram in THB (updated when gold price changes).</summary>
    public decimal UnitCostThbPerGram { get; set; }

    public int SortOrder { get; set; }
}
