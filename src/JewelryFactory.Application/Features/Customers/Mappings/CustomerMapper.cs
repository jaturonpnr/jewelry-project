using JewelryFactory.Application.Features.Customers.DTOs;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.ValueObjects;

namespace JewelryFactory.Application.Features.Customers.Mappings;

/// <summary>
/// Manual mapping (small/explicit, no Mapster runtime cost).
/// Switch to Mapster TypeAdapterConfig if mappings grow large.
/// </summary>
internal static class CustomerMapper
{
    public static CustomerResponseDto ToResponse(this Customer c) => new(
        c.Id,
        c.Code,
        c.CompanyName,
        c.ContactPerson,
        c.Email,
        c.Phone,
        c.TaxId,
        c.Type.ToString(),
        c.DefaultCurrency.ToString(),
        c.PaymentTerms.ToString(),
        c.CreditLimit,
        c.IsActive,
        new AddressDto(c.Address.Line1, c.Address.Line2, c.Address.City,
            c.Address.State, c.Address.PostalCode, c.Address.Country),
        c.Notes,
        c.CreatedAt
    );

    public static Address ToValueObject(this AddressDto dto) => new()
    {
        Line1 = dto.Line1,
        Line2 = dto.Line2,
        City = dto.City,
        State = dto.State,
        PostalCode = dto.PostalCode,
        Country = dto.Country
    };
}
