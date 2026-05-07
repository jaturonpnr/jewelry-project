using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.Reports.DTOs;
using JewelryFactory.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Reports.Queries;

public record GetSalesOrderPipelineReportQuery() : IRequest<SalesOrderPipelineReportDto>;

public class GetSalesOrderPipelineReportHandler(IApplicationDbContext db)
    : IRequestHandler<GetSalesOrderPipelineReportQuery, SalesOrderPipelineReportDto>
{
    private const int TopN = 10;

    public async Task<SalesOrderPipelineReportDto> Handle(GetSalesOrderPipelineReportQuery query, CancellationToken ct)
    {
        // ── Status breakdown (group by Status; if multi-currency, currency = null) ─────
        var statusGroups = await db.SalesOrders.AsNoTracking()
            .GroupBy(o => o.Status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count(),
                TotalAmount = g.Sum(o => o.TotalAmount),
                Currencies = g.Select(o => o.Currency).Distinct().ToList()
            })
            .ToListAsync(ct);

        var byStatus = statusGroups
            .Select(g => new OrdersByStatusDto(
                g.Status.ToString(),
                g.Count,
                g.TotalAmount,
                g.Currencies.Count == 1 ? g.Currencies[0].ToString() : null))
            .OrderBy(x => x.Status)
            .ToList();

        // ── Top customers by order count ────────────────────────────────────────────
        var topCustomers = await db.SalesOrders.AsNoTracking()
            .Include(o => o.Customer)
            .GroupBy(o => new { o.CustomerId, o.Customer.Code, o.Customer.CompanyName })
            .Select(g => new
            {
                g.Key.CustomerId,
                g.Key.Code,
                g.Key.CompanyName,
                OrderCount = g.Count(),
                TotalRevenue = g.Sum(o => o.TotalAmount),
                DominantCurrency = g.GroupBy(o => o.Currency)
                    .OrderByDescending(cg => cg.Count())
                    .Select(cg => cg.Key)
                    .First()
            })
            .OrderByDescending(g => g.OrderCount)
            .ThenByDescending(g => g.TotalRevenue)
            .Take(TopN)
            .ToListAsync(ct);

        var topCustomersDto = topCustomers
            .Select(c => new OrdersByCustomerDto(
                c.CustomerId, c.Code, c.CompanyName,
                c.OrderCount, c.TotalRevenue, c.DominantCurrency.ToString()))
            .ToList();

        // ── Revenue by currency ──────────────────────────────────────────────────────
        var byCurrencyRaw = await db.SalesOrders.AsNoTracking()
            .GroupBy(o => o.Currency)
            .Select(g => new
            {
                Currency = g.Key,
                OrderCount = g.Count(),
                TotalRevenue = g.Sum(o => o.TotalAmount)
            })
            .ToListAsync(ct);
        var byCurrency = byCurrencyRaw
            .Select(x => new OrdersByCurrencyDto(x.Currency.ToString(), x.OrderCount, x.TotalRevenue))
            .OrderBy(x => x.Currency)
            .ToList();

        // ── Overdue orders (RequestedDeliveryDate < today, not Delivered/Cancelled) ─
        var today = DateTime.UtcNow.Date;
        var overdue = await db.SalesOrders.AsNoTracking()
            .Include(o => o.Customer)
            .Where(o => o.RequestedDeliveryDate.HasValue
                && o.RequestedDeliveryDate.Value < today
                && o.Status != SalesOrderStatus.Delivered
                && o.Status != SalesOrderStatus.Cancelled)
            .OrderBy(o => o.RequestedDeliveryDate)
            .Take(TopN)
            .ToListAsync(ct);

        var overdueDto = overdue
            .Select(o => new OverdueOrderDto(
                o.Id,
                o.OrderNumber,
                o.Customer.CompanyName,
                o.Status.ToString(),
                o.RequestedDeliveryDate!.Value,
                (today - o.RequestedDeliveryDate.Value.Date).Days,
                o.Currency.ToString(),
                o.TotalAmount))
            .ToList();

        // ── Counts ───────────────────────────────────────────────────────────────────
        var totalOrders = await db.SalesOrders.AsNoTracking().CountAsync(ct);
        var openOrders = await db.SalesOrders.AsNoTracking()
            .CountAsync(o => o.Status != SalesOrderStatus.Delivered
                          && o.Status != SalesOrderStatus.Cancelled, ct);

        return new SalesOrderPipelineReportDto(
            byStatus, topCustomersDto, byCurrency, overdueDto,
            totalOrders, openOrders);
    }
}
