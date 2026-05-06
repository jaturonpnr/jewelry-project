using JewelryFactory.Domain.Common;
using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Domain.Entities;

/// <summary>
/// Lookup of all material kinds the factory uses (Gold 18k, Diamond, Findings, etc.)
/// Inventory items reference this for type, unit, and (for metals) default purity.
/// </summary>
public class MaterialType : AuditableEntity
{
    public required string Code { get; set; }                    // e.g. GOLD_18K, DIAMOND_GIA, RING_FINDING
    public required string Name { get; set; }                    // display name

    public MaterialCategory Category { get; set; }
    public MaterialUnit Unit { get; set; }

    // For metals only — null otherwise
    public decimal? PurityFraction { get; set; }                 // 0.0001 .. 1.0000
    public int? Karat { get; set; }                              // 9, 14, 18, 22, 24

    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
}
