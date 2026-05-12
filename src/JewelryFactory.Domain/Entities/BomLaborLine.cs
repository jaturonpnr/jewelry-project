using JewelryFactory.Domain.Common;
using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Domain.Entities;

/// <summary>
/// Labour cost for one production stage within a BOM.
/// </summary>
public class BomLaborLine : BaseEntity
{
    public Guid BomTemplateId { get; set; }
    public BomTemplate BomTemplate { get; set; } = null!;

    public ProductionStage Stage { get; set; }
    public decimal EstimatedHours { get; set; }
    public decimal HourlyRateThb { get; set; }

    // Computed (not stored)
    public decimal LaborCostThb => EstimatedHours * HourlyRateThb;
}
