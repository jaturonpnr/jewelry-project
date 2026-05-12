using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Enums;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Shipping.Commands.UpdateInvoiceStatus;

public class UpdateInvoiceStatusHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateInvoiceStatusCommand>
{
    public async Task Handle(UpdateInvoiceStatusCommand command, CancellationToken ct)
    {
        var invoice = await db.Invoices
            .FirstOrDefaultAsync(i => i.Id == command.Id && !i.IsDeleted, ct)
            ?? throw new NotFoundException(nameof(Invoice), command.Id);

        var req = command.Request;

        if (!InvoiceStatusFlow.CanTransition(invoice.Status, req.NewStatus))
            throw new InvalidOperationException(
                $"Cannot transition invoice from {invoice.Status} to {req.NewStatus}.");

        invoice.Status = req.NewStatus;

        if (req.NewStatus == InvoiceStatus.Paid)
        {
            invoice.PaidAt = req.PaidAt?.ToUniversalTime() ?? DateTime.UtcNow;
            invoice.PaymentReference = req.PaymentReference?.Trim();
        }

        await db.SaveChangesAsync(ct);
    }
}
