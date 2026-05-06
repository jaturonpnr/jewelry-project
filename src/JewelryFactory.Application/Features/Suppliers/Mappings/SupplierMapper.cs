using JewelryFactory.Application.Features.Customers.DTOs;
using JewelryFactory.Application.Features.Customers.Mappings; // for AddressDto.ToValueObject()
using JewelryFactory.Application.Features.Suppliers.DTOs;
using JewelryFactory.Domain.Entities;

namespace JewelryFactory.Application.Features.Suppliers.Mappings;

internal static class SupplierMapper
{
    public static SupplierResponseDto ToResponse(this Supplier s) => new(
        s.Id, s.Code, s.CompanyName, s.ContactPerson, s.Email, s.Phone, s.TaxId,
        s.Type.ToString(), s.DefaultCurrency.ToString(), s.PaymentTerms.ToString(),
        s.IsActive,
        new AddressDto(s.Address.Line1, s.Address.Line2, s.Address.City,
            s.Address.State, s.Address.PostalCode, s.Address.Country),
        s.Certifications, s.Notes, s.CreatedAt
    );
}
