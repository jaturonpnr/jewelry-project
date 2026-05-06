using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Application.Features.Customers.DTOs;

public record AddressDto(
    string? Line1, string? Line2, string? City,
    string? State, string? PostalCode, string? Country
);

public record CreateCustomerDto(
    string Code,
    string CompanyName,
    string? ContactPerson,
    string? Email,
    string? Phone,
    string? TaxId,
    CustomerType Type,
    Currency DefaultCurrency,
    PaymentTerms PaymentTerms,
    decimal CreditLimit,
    AddressDto Address,
    string? Notes
);

public record UpdateCustomerDto(
    string CompanyName,
    string? ContactPerson,
    string? Email,
    string? Phone,
    string? TaxId,
    CustomerType Type,
    Currency DefaultCurrency,
    PaymentTerms PaymentTerms,
    decimal CreditLimit,
    bool IsActive,
    AddressDto Address,
    string? Notes
);

public record CustomerResponseDto(
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
    decimal CreditLimit,
    bool IsActive,
    AddressDto Address,
    string? Notes,
    DateTime CreatedAt
);
