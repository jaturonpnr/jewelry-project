using JewelryFactory.Domain.Common;

namespace JewelryFactory.Domain.Entities;

/// <summary>
/// Bill of Materials template for a jewelry design.
/// Captures expected materials, stones, and labour per piece.
/// Phase 2 - Module 7.
/// </summary>
public class BomTemplate : AuditableEntity
{
    public required string DesignCode { get; set; }    // e.g. "RNG-18K-D001"
    public required string DesignName { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>Overhead % applied on top of material + labour cost.</summary>
    public decimal OverheadPercent { get; set; } = 15m;

    public ICollection<BomMaterialLine> MaterialLines { get; set; } = new List<BomMaterialLine>();
    public ICollection<BomStoneLine> StoneLines { get; set; } = new List<BomStoneLine>();
    public ICollection<BomLaborLine> LaborLines { get; set; } = new List<BomLaborLine>();
}
