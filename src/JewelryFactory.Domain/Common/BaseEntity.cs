namespace JewelryFactory.Domain.Common;

/// <summary>
/// Base entity. Use Guid Id by default for distributed-friendly identifiers.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
