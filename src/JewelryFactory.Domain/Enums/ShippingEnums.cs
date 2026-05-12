namespace JewelryFactory.Domain.Enums;

public enum ShipmentStatus
{
    Preparing = 1,
    Shipped = 2,
    InTransit = 3,
    Delivered = 4,
    Returned = 5,
}

public enum InvoiceStatus
{
    Draft = 1,
    Sent = 2,
    Paid = 3,
    Overdue = 4,
    Cancelled = 99,
}

public static class InvoiceStatusFlow
{
    private static readonly Dictionary<InvoiceStatus, InvoiceStatus[]> Allowed = new()
    {
        [InvoiceStatus.Draft]     = [InvoiceStatus.Sent,    InvoiceStatus.Cancelled],
        [InvoiceStatus.Sent]      = [InvoiceStatus.Paid,    InvoiceStatus.Overdue, InvoiceStatus.Cancelled],
        [InvoiceStatus.Overdue]   = [InvoiceStatus.Paid,    InvoiceStatus.Cancelled],
        [InvoiceStatus.Paid]      = [],
        [InvoiceStatus.Cancelled] = [],
    };

    public static bool CanTransition(InvoiceStatus from, InvoiceStatus to)
        => Allowed.TryGetValue(from, out var next) && next.Contains(to);
}
