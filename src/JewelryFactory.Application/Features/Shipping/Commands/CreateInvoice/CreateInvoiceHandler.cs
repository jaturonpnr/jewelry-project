using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Shipping.Commands.CreateInvoice;

public class CreateInvoiceHandler(IApplicationDbContext db)
    : IRequestHandler<CreateInvoiceCommand, Guid>
{
    public async Task<Guid> Handle(CreateInvoiceCommand command, CancellationToken ct)
    {
        var req = command.Request;
        var number = await GenerateInvoiceNumberAsync(ct);

        var invoice = new Invoice
        {
            InvoiceNumber = number,
            CustomerId = req.CustomerId,
            SalesOrderId = req.SalesOrderId,
            ShipmentId = req.ShipmentId,
            InvoiceDate = req.InvoiceDate.ToUniversalTime(),
            DueDate = req.DueDate.ToUniversalTime(),
            Currency = req.Currency,
            ExchangeRateToThb = req.ExchangeRateToThb,
            VatApplicable = req.VatApplicable,
            PaymentTerms = req.PaymentTerms?.Trim(),
            Notes = req.Notes?.Trim(),
        };

        int lineNum = 1;
        foreach (var l in req.LineItems)
        {
            var lineTotal = l.Quantity * l.UnitPrice * (1 - l.DiscountPercent / 100m);
            invoice.LineItems.Add(new InvoiceLineItem
            {
                LineNumber = lineNum++,
                Description = l.Description.Trim(),
                DesignCode = l.DesignCode?.Trim(),
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                DiscountPercent = l.DiscountPercent,
                LineTotal = Math.Round(lineTotal, 2),
            });
        }

        RecalculateTotals(invoice);

        db.Invoices.Add(invoice);
        await db.SaveChangesAsync(ct);
        return invoice.Id;
    }

    private static void RecalculateTotals(Invoice invoice)
    {
        invoice.Subtotal = invoice.LineItems.Sum(l => l.LineTotal);
        invoice.DiscountAmount = 0; // line-level discounts already applied
        invoice.VatAmount = invoice.VatApplicable
            ? Math.Round(invoice.Subtotal * (invoice.VatPercent / 100m), 2)
            : 0;
        invoice.TotalAmount = invoice.Subtotal + invoice.VatAmount;
        invoice.TotalAmountThb = Math.Round(invoice.TotalAmount * invoice.ExchangeRateToThb, 2);
    }

    private async Task<string> GenerateInvoiceNumberAsync(CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var count = await db.Invoices.CountAsync(ct) + 1;
        return $"INV-{year}-{count:D4}";
    }
}
