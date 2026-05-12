using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.Shipping.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Shipping.Queries.GetInvoicesPaged;

public class GetInvoicesPagedHandler(IApplicationDbContext db)
    : IRequestHandler<GetInvoicesPagedQuery, PagedResult<InvoiceSummary>>
{
    public async Task<PagedResult<InvoiceSummary>> Handle(
        GetInvoicesPagedQuery query, CancellationToken ct)
    {
        var q = db.Invoices.AsNoTracking()
            .Where(i => !i.IsDeleted)
            .Include(i => i.Customer)
            .Include(i => i.SalesOrder)
            .AsQueryable();

        if (query.Status.HasValue)
            q = q.Where(i => i.Status == query.Status.Value);

        if (query.CustomerId.HasValue)
            q = q.Where(i => i.CustomerId == query.CustomerId.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(i => i.InvoiceNumber.ToLower().Contains(term) ||
                              i.Customer.CompanyName.ToLower().Contains(term));
        }

        var totalCount = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(i => i.InvoiceDate)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(i => new InvoiceSummary(
                i.Id, i.InvoiceNumber,
                i.Customer.CompanyName,
                i.SalesOrder != null ? i.SalesOrder.OrderNumber : null,
                i.Status, i.InvoiceDate, i.DueDate,
                i.Currency, i.TotalAmount, i.TotalAmountThb, i.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<InvoiceSummary>
        {
            Items = items, TotalCount = totalCount, Page = query.Page, PageSize = query.PageSize,
        };
    }
}
