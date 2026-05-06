namespace JewelryFactory.Domain.Common;

/// <summary>
/// Base entity with audit trail. Required for all business entities per CLAUDE.md §7.
/// Soft delete only — never hard delete (legal/audit reasons).
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "system";
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
