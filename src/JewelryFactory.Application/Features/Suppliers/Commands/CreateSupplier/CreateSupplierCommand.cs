using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.Customers.Mappings;
using JewelryFactory.Application.Features.Suppliers.DTOs;
using JewelryFactory.Application.Features.Suppliers.Mappings;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Suppliers.Commands.CreateSupplier;

public record CreateSupplierCommand(CreateSupplierDto Request) : IRequest<SupplierResponseDto>;

public class CreateSupplierHandler(IApplicationDbContext db)
    : IRequestHandler<CreateSupplierCommand, SupplierResponseDto>
{
    public async Task<SupplierResponseDto> Handle(CreateSupplierCommand command, CancellationToken ct)
    {
        var dto = command.Request;
        var code = dto.Code.Trim().ToUpperInvariant();

        if (await db.Suppliers.AnyAsync(s => s.Code == code, ct))
            throw new BusinessRuleException($"Supplier code '{code}' is already in use.");

        var supplier = new Supplier
        {
            Code = code,
            CompanyName = dto.CompanyName.Trim(),
            ContactPerson = dto.ContactPerson?.Trim(),
            Email = dto.Email?.Trim().ToLowerInvariant(),
            Phone = dto.Phone?.Trim(),
            TaxId = dto.TaxId?.Trim(),
            Type = dto.Type,
            DefaultCurrency = dto.DefaultCurrency,
            PaymentTerms = dto.PaymentTerms,
            Address = dto.Address.ToValueObject(),
            Certifications = dto.Certifications?.Trim(),
            Notes = dto.Notes?.Trim()
        };

        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(ct);
        return supplier.ToResponse();
    }
}
