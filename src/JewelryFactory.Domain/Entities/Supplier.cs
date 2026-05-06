using JewelryFactory.Domain.Common;
using JewelryFactory.Domain.Enums;
using JewelryFactory.Domain.ValueObjects;

namespace JewelryFactory.Domain.Entities;

public class Supplier : AuditableEntity
{
    public required string Code { get; set; }            // unique, e.g. SUP-0001
    public required string CompanyName { get; set; }
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? TaxId { get; set; }

    public SupplierType Type { get; set; } = SupplierType.Other;
    public Currency DefaultCurrency { get; set; } = Currency.USD;
    public PaymentTerms PaymentTerms { get; set; } = PaymentTerms.Net30;

    public bool IsActive { get; set; } = true;

    public Address Address { get; set; } = new();
    public string? Certifications { get; set; }   // free text e.g. "RJC, Kimberley Process"
    public string? Notes { get; set; }
}
