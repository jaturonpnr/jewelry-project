using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.Customers.Mappings;
using JewelryFactory.Application.Features.Suppliers.DTOs;
using JewelryFactory.Application.Features.Suppliers.Mappings;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Suppliers.Commands.UpdateSupplier;

public record UpdateSupplierCommand(Guid Id, UpdateSupplierDto Request) : IRequest<SupplierResponseDto>;

public class UpdateSupplierHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateSupplierCommand, SupplierResponseDto>
{
    public async Task<SupplierResponseDto> Handle(UpdateSupplierCommand command, CancellationToken ct)
    {
        var supplier = await db.Suppliers.FirstOrDefaultAsync(s => s.Id == command.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Supplier), command.Id);

        var dto = command.Request;
        supplier.CompanyName = dto.CompanyName.Trim();
        supplier.ContactPerson = dto.ContactPerson?.Trim();
        supplier.Email = dto.Email?.Trim().ToLowerInvariant();
        supplier.Phone = dto.Phone?.Trim();
        supplier.TaxId = dto.TaxId?.Trim();
        supplier.Type = dto.Type;
        supplier.DefaultCurrency = dto.DefaultCurrency;
        supplier.PaymentTerms = dto.PaymentTerms;
        supplier.IsActive = dto.IsActive;
        supplier.Address = dto.Address.ToValueObject();
        supplier.Certifications = dto.Certifications?.Trim();
        supplier.Notes = dto.Notes?.Trim();

        await db.SaveChangesAsync(ct);
        return supplier.ToResponse();
    }
}
