using JewelryFactory.Domain.Common;
using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Domain.Entities;

/// <summary>
/// Completed jewelry item ready for sale/shipment.
/// Phase 1 — minimal fields; Design + BOM linkage will be added in Phase 2.
/// </summary>
public class FinishedGoods : ConcurrentEntity
{
    public required string SerialNumber { get; set; }       // unique business identifier
    public string? DesignCode { get; set; }                  // future link to Design

    public required string Description { get; set; }
    public decimal TotalGoldWeightGrams { get; set; }        // total gold (gross)
    public decimal TotalStoneCarat { get; set; }             // sum of stones
    public string? StoneSummary { get; set; }                // free text or json

    public decimal TotalCost { get; set; }
    public decimal? ListPrice { get; set; }
    public Currency Currency { get; set; } = Currency.USD;

    public string Location { get; set; } = "MAIN";
    public StockStatus Status { get; set; } = StockStatus.InStock;
    public DateTime CompletedDate { get; set; }
    public string? Notes { get; set; }
}
