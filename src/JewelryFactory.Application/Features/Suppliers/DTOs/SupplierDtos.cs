using JewelryFactory.Application.Features.Customers.DTOs; // reuse AddressDto
using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Application.Features.Suppliers.DTOs;

public record CreateSupplierDto(
    string Code,
    string CompanyName,
    string? ContactPerson,
    string? Email,
    string? Phone,
    string? TaxId,
    SupplierType Type,
    Currency DefaultCurrency,
    PaymentTerms PaymentTerms,
    AddressDto Address,
    string? Certifications,
    string? Notes
);

public record UpdateSupplierDto(
    string CompanyName,
    string? ContactPerson,
    string? Email,
    string? Phone,
    string? TaxId,
    SupplierType Type,
    Currency DefaultCurrency,
    PaymentTerms PaymentTerms,
    bool IsActive,
    AddressDto Address,
    string? Certifications,
    string? Notes
);

public record SupplierResponseDto(
    Guid Id,
    string Code,
    string CompanyName,
    string? ContactPerson,
    string? Email,
    string? Phone,
    string? TaxId,
    string Type,
    string DefaultCurrency,
    string PaymentTerms,
    bool IsActive,
    AddressDto Address,
    string? Certifications,
    string? Notes,
    DateTime CreatedAt
);
