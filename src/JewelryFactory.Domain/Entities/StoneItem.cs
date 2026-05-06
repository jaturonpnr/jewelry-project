using JewelryFactory.Domain.Common;
using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Domain.Entities;

/// <summary>
/// Individual stone — stones with carat ≥ 0.20 OR with certificate.
/// Tracked one-by-one (per CLAUDE.md §10.4).
/// </summary>
public class StoneItem : ConcurrentEntity
{
    public required string ItemCode { get; set; }       // unique business code, e.g. STN-2026-0001

    public required Guid MaterialTypeId { get; set; }   // e.g. DIAMOND, RUBY
    public MaterialType MaterialType { get; set; } = null!;

    public decimal CaratWeight { get; set; }            // 4 decimals
    public StoneShape Shape { get; set; } = StoneShape.Round;
    public StoneColor? Color { get; set; }
    public StoneClarity? Clarity { get; set; }
    public string? Cut { get; set; }                    // Excellent, Very Good, Good, Fair
    public string? Measurements { get; set; }           // e.g. "5.20 x 5.22 x 3.18 mm"

    public StoneCertAuthority CertAuthority { get; set; } = StoneCertAuthority.None;
    public string? CertificateNumber { get; set; }
    public string? Origin { get; set; }                 // e.g. "Botswana"

    public Guid? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public DateTime ReceivedDate { get; set; }
    public string Location { get; set; } = "MAIN";

    public decimal UnitCost { get; set; }
    public Currency CostCurrency { get; set; } = Currency.USD;

    public StockStatus Status { get; set; } = StockStatus.InStock;
    public string? Notes { get; set; }
}
