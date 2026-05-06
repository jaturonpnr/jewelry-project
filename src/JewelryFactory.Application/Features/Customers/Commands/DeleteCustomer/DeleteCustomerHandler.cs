using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Customers.Commands.DeleteCustomer;

/// <summary>
/// Soft-delete handled automatically by DbContext.SaveChangesAsync (per CLAUDE.md §10.10).
/// </summary>
public class DeleteCustomerHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteCustomerCommand>
{
    public async Task Handle(DeleteCustomerCommand command, CancellationToken ct)
    {
        var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == command.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Customer), command.Id);

        db.Customers.Remove(customer);  // intercepted → soft delete
        await db.SaveChangesAsync(ct);
    }
}
