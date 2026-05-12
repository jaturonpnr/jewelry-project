namespace JewelryFactory.Domain.Enums;

/// <summary>
/// Three QC checkpoints in jewelry production.
/// </summary>
public enum QcInspectionType
{
    Incoming = 1,     // raw material / stone received from supplier
    InProcess = 2,    // mid-production stage check
    Final = 3,        // before packaging / shipment
}

public enum QcResult
{
    Pass = 1,
    Rework = 2,       // send back for repair
    Fail = 3,         // reject / scrap
}

public enum DefectSeverity
{
    Minor = 1,        // cosmetic — does not affect function
    Major = 2,        // affects quality, rework required
    Critical = 3,     // safety / structural — automatic fail
}
