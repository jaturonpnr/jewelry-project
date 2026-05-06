using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.Suppliers.DTOs;
using JewelryFactory.Application.Features.Suppliers.Mappings;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Suppliers.Queries.GetSupplierById;

public record GetSupplierByIdQuery(Guid Id) : IRequest<SupplierResponseDto>;

public class GetSupplierByIdHandler(IApplicationDbContext db)
    : IRequestHandler<GetSupplierByIdQuery, SupplierResponseDto>
{
    public async Task<SupplierResponseDto> Handle(GetSupplierByIdQuery query, CancellationToken ct)
    {
        var supplier = await db.Suppliers.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == query.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Supplier), query.Id);

        return supplier.ToResponse();
    }
}
