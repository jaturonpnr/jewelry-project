using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Suppliers.Commands.DeleteSupplier;

public record DeleteSupplierCommand(Guid Id) : IRequest;

public class DeleteSupplierHandler(IApplicationDbContext db) : IRequestHandler<DeleteSupplierCommand>
{
    public async Task Handle(DeleteSupplierCommand command, CancellationToken ct)
    {
        var supplier = await db.Suppliers.FirstOrDefaultAsync(s => s.Id == command.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Supplier), command.Id);

        db.Suppliers.Remove(supplier);
        await db.SaveChangesAsync(ct);
    }
}
