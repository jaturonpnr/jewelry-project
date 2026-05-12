using JewelryFactory.Domain.Common;
using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Domain.Entities;

/// <summary>
/// Quality-control inspection record.
/// Covers incoming materials, in-process stage checks, and final pre-ship QC.
/// Phase 2 - Module 8.
/// </summary>
public class QcInspection : AuditableEntity
{
    public QcInspectionType InspectionType { get; set; }
    public DateTime InspectionDate { get; set; } = DateTime.UtcNow;

    // ── Links (all optional — depends on InspectionType) ──────────────────

    /// <summary>Work order being inspected (InProcess / Final).</summary>
    public Guid? WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }

    /// <summary>Specific stage being inspected (InProcess only).</summary>
    public Guid? WorkOrderStageId { get; set; }
    public WorkOrderStage? WorkOrderStage { get; set; }

    /// <summary>Raw material batch being inspected (Incoming only).</summary>
    public Guid? RawMaterialItemId { get; set; }
    public RawMaterialItem? RawMaterialItem { get; set; }

    // ── Inspector ─────────────────────────────────────────────────────────

    public Guid? InspectorId { get; set; }
    public Worker? Inspector { get; set; }

    /// <summary>Captured at time of inspection in case worker is later deactivated.</summary>
    public string InspectorName { get; set; } = string.Empty;

    // ── Result ────────────────────────────────────────────────────────────

    public QcResult Result { get; set; }
    public string? Notes { get; set; }

    /// <summary>
    /// For Rework results: which stage should be re-done.
    /// Null for Pass / Fail.
    /// </summary>
    public ProductionStage? ReworkStage { get; set; }

    // ── Measurements (optional — for weight-based QC) ─────────────────────

    public decimal? ActualWeightGrams { get; set; }
    public decimal? ExpectedWeightGrams { get; set; }

    public ICollection<QcDefect> Defects { get; set; } = new List<QcDefect>();
}
