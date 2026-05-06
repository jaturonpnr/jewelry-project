using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.Customers.DTOs;
using JewelryFactory.Application.Features.Customers.Mappings;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerHandler(IApplicationDbContext db)
    : IRequestHandler<CreateCustomerCommand, CustomerResponseDto>
{
    public async Task<CustomerResponseDto> Handle(CreateCustomerCommand command, CancellationToken ct)
    {
        var dto = command.Request;
        var code = dto.Code.Trim().ToUpperInvariant();

        var exists = await db.Customers.AnyAsync(c => c.Code == code, ct);
        if (exists)
            throw new BusinessRuleException($"Customer code '{code}' is already in use.");

        var customer = new Customer
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
            CreditLimit = dto.CreditLimit,
            Address = dto.Address.ToValueObject(),
            Notes = dto.Notes?.Trim()
        };

        db.Customers.Add(customer);
        await db.SaveChangesAsync(ct);

        return customer.ToResponse();
    }
}
