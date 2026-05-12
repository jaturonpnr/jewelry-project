namespace JewelryFactory.Domain.Enums;

/// <summary>
/// Per CLAUDE.md §10.4: stones ≥0.20ct or with certificate → Individual;
/// stones &lt;0.20ct → Parcel.
/// </summary>
public enum StoneTrackingType
{
    Individual = 1,
    Parcel = 2,
}
