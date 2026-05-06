using JewelryFactory.Domain.Common;
using JewelryFactory.Domain.Enums;
using JewelryFactory.Domain.ValueObjects;

namespace JewelryFactory.Domain.Entities;

/// <summary>
/// B2B customer (wholesaler/retailer/catalogue publisher).
/// Phase 1 - Module 2 (Master Data).
/// </summary>
public class Customer : AuditableEntity
{
    public required string Code { get; set; }            // unique business code, e.g. CUST-0001
    public required string CompanyName { get; set; }
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? TaxId { get; set; }

    public CustomerType Type { get; set; } = CustomerType.Wholesaler;
    public Currency DefaultCurrency { get; set; } = Currency.USD;
    public PaymentTerms PaymentTerms { get; set; } = PaymentTerms.Net30;

    public decimal CreditLimit { get; set; }             // in DefaultCurrency
    public bool IsActive { get; set; } = true;

    public Address Address { get; set; } = new();
    public string? Notes { get; set; }
}
