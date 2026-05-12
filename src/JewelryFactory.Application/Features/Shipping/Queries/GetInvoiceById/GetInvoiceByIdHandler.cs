using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.Shipping.DTOs;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Shipping.Queries.GetInvoiceById;

public class GetInvoiceByIdHandler(IApplicationDbContext db)
    : IRequestHandler<GetInvoiceByIdQuery, InvoiceResponse>
{
    public async Task<InvoiceResponse> Handle(GetInvoiceByIdQuery query, CancellationToken ct)
    {
        var i = await db.Invoices.AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.SalesOrder)
            .Include(x => x.Shipment)
            .Include(x => x.LineItems)
            .FirstOrDefaultAsync(x => x.Id == query.Id && !x.IsDeleted, ct)
            ?? throw new NotFoundException(nameof(Invoice), query.Id);

        return new InvoiceResponse(
            i.Id, i.InvoiceNumber,
            i.CustomerId, i.Customer.CompanyName,
            i.SalesOrderId, i.SalesOrder?.OrderNumber,
            i.ShipmentId, i.Shipment?.ShipmentNumber,
            i.Status, i.InvoiceDate, i.DueDate,
            i.Currency, i.Currency.ToString(),
            i.ExchangeRateToThb,
            i.VatApplicable, i.VatPercent,
            i.PaymentTerms, i.Notes,
            i.PaidAt, i.PaymentReference,
            i.Subtotal, i.DiscountAmount, i.VatAmount,
            i.TotalAmount, i.TotalAmountThb,
            i.LineItems.OrderBy(l => l.LineNumber)
                .Select(l => new InvoiceLineItemResponse(
                    l.Id, l.LineNumber, l.Description, l.DesignCode,
                    l.Quantity, l.UnitPrice, l.DiscountPercent, l.LineTotal))
                .ToList(),
            i.CreatedAt);
    }
}
