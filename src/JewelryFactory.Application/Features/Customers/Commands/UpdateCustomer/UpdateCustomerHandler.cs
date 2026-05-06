using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.Customers.DTOs;
using JewelryFactory.Application.Features.Customers.Mappings;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Customers.Commands.UpdateCustomer;

public class UpdateCustomerHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateCustomerCommand, CustomerResponseDto>
{
    public async Task<CustomerResponseDto> Handle(UpdateCustomerCommand command, CancellationToken ct)
    {
        var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == command.Id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Customer), command.Id);

        var dto = command.Request;
        customer.CompanyName = dto.CompanyName.Trim();
        customer.ContactPerson = dto.ContactPerson?.Trim();
        customer.Email = dto.Email?.Trim().ToLowerInvariant();
        customer.Phone = dto.Phone?.Trim();
        customer.TaxId = dto.TaxId?.Trim();
        customer.Type = dto.Type;
        customer.DefaultCurrency = dto.DefaultCurrency;
        customer.PaymentTerms = dto.PaymentTerms;
        customer.CreditLimit = dto.CreditLimit;
        customer.IsActive = dto.IsActive;
        customer.Address = dto.Address.ToValueObject();
        customer.Notes = dto.Notes?.Trim();

        await db.SaveChangesAsync(ct);
        return customer.ToResponse();
    }
}
