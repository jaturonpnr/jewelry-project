namespace JewelryFactory.Application.Features.Reports.DTOs;

public record SalesOrderPipelineReportDto(
    IReadOnlyList<OrdersByStatusDto> ByStatus,
    IReadOnlyList<OrdersByCustomerDto> TopCustomers,
    IReadOnlyList<OrdersByCurrencyDto> RevenueByCurrency,
    IReadOnlyList<OverdueOrderDto> Overdue,
    int TotalOrders,
    int OpenOrders                     // not Delivered or Cancelled
);

public record OrdersByStatusDto(
    string Status,
    int Count,
    decimal TotalAmount,               // sum (in original currency — see RevenueByCurrency for cross-currency)
    string? Currency                   // null when mixed currencies
);

public record OrdersByCustomerDto(
    Guid CustomerId,
    string CustomerCode,
    string CustomerName,
    int OrderCount,
    decimal TotalRevenue,
    string Currency                    // dominant currency
);

public record OrdersByCurrencyDto(
    string Currency,
    int OrderCount,
    decimal TotalRevenue
);

public record OverdueOrderDto(
    Guid OrderId,
    string OrderNumber,
    string CustomerName,
    string Status,
    DateTime RequestedDeliveryDate,
    int DaysOverdue,
    string Currency,
    decimal TotalAmount
);
