namespace JewelryFactory.Domain.Common;

/// <summary>
/// AuditableEntity + RowVersion for optimistic concurrency.
/// Required for any entity whose stock/quantity can be mutated concurrently
/// (per CLAUDE.md §12.7).
/// </summary>
public abstract class ConcurrentEntity : AuditableEntity
{
    public byte[] RowVersion { get; set; } = [];
}
