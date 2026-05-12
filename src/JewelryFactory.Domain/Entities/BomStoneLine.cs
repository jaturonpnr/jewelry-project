using JewelryFactory.Domain.Common;
using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Domain.Entities;

/// <summary>
/// Gemstone requirement in a BOM.
/// Tracking type follows the ≥0.20ct rule from CLAUDE.md §10.4.
/// </summary>
public class BomStoneLine : BaseEntity
{
    public Guid BomTemplateId { get; set; }
    public BomTemplate BomTemplate { get; set; } = null!;

    public required string StoneType { get; set; }         // "Diamond", "Ruby", "Sapphire" …
    public required string StoneShape { get; set; }        // "Round Brilliant", "Princess" …
    public required string SizeDescription { get; set; }   // "0.50ct RB D/VVS1"

    /// <summary>Carat weight per stone.</summary>
    public decimal CaratPerStone { get; set; }

    /// <summary>Number of stones per finished piece.</summary>
    public int Quantity { get; set; }

    public StoneTrackingType TrackingType { get; set; }

    /// <summary>Cost per carat in THB.</summary>
    public decimal UnitCostThbPerCarat { get; set; }

    public int SortOrder { get; set; }

    // Computed helper (not stored)
    public decimal TotalCaratWeight => CaratPerStone * Quantity;
    public decimal LineCostThb => TotalCaratWeight * UnitCostThbPerCarat;
}
