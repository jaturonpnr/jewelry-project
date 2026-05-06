namespace JewelryFactory.Domain.Enums;

/// <summary>
/// Sales order lifecycle (CLAUDE.md §10.9).
/// Strict transitions enforced via SalesOrderStatusFlow.
/// </summary>
public enum SalesOrderStatus
{
    Draft = 1,
    Confirmed = 2,
    InProduction = 3,
    Qc = 4,
    Packed = 5,
    Shipped = 6,
    Delivered = 7,
    Cancelled = 99
}

/// <summary>
/// Allowed transitions per CLAUDE.md §10.9.
/// </summary>
public static class SalesOrderStatusFlow
{
    private static readonly Dictionary<SalesOrderStatus, SalesOrderStatus[]> Allowed = new()
    {
        [SalesOrderStatus.Draft]        = [SalesOrderStatus.Confirmed,    SalesOrderStatus.Cancelled],
        [SalesOrderStatus.Confirmed]    = [SalesOrderStatus.InProduction, SalesOrderStatus.Cancelled],
        [SalesOrderStatus.InProduction] = [SalesOrderStatus.Qc,           SalesOrderStatus.Cancelled],
        [SalesOrderStatus.Qc]           = [SalesOrderStatus.Packed,       SalesOrderStatus.Cancelled],
        [SalesOrderStatus.Packed]       = [SalesOrderStatus.Shipped,      SalesOrderStatus.Cancelled],
        [SalesOrderStatus.Shipped]      = [SalesOrderStatus.Delivered,    SalesOrderStatus.Cancelled],
        [SalesOrderStatus.Delivered]    = [],
        [SalesOrderStatus.Cancelled]    = [],
    };

    public static bool CanTransition(SalesOrderStatus from, SalesOrderStatus to)
        => Allowed.TryGetValue(from, out var next) && next.Contains(to);
}
