using JewelryFactory.Domain.Common;
using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Domain.Entities;

/// <summary>
/// A single defect found during a QC inspection.
/// </summary>
public class QcDefect : BaseEntity
{
    public Guid QcInspectionId { get; set; }
    public QcInspection QcInspection { get; set; } = null!;

    public required string DefectType { get; set; }   // "Scratch", "Porosity", "Loose Stone" …
    public DefectSeverity Severity { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; } = 1;
}
