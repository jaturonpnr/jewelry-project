using JewelryFactory.Domain.Common;
using JewelryFactory.Domain.Enums;

namespace JewelryFactory.Domain.Entities;

/// <summary>
/// Commercial invoice.
/// VAT 7% applies for THB domestic transactions (CLAUDE.md §2).
/// Phase 2 - Module 11.
/// </summary>
public class Invoice : AuditableEntity
{
    public required string InvoiceNumber { get; set; }  // INV-2026-0001

    public required Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public Guid? SalesOrderId { get; set; }
    public SalesOrder? SalesOrder { get; set; }

    public Guid? ShipmentId { get; set; }
    public Shipment? Shipment { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }

    public Currency Currency { get; set; } = Currency.USD;

    /// <summary>Exchange rate to THB at invoice date (captured as snapshot).</summary>
    public decimal ExchangeRateToThb { get; set; } = 1m;

    /// <summary>Whether 7% VAT applies (Thai domestic transactions).</summary>
    public bool VatApplicable { get; set; }
    public decimal VatPercent { get; set; } = 7m;

    public string? PaymentTerms { get; set; }          // "Net 30", "50% deposit"
    public string? Notes { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? PaymentReference { get; set; }

    public ICollection<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();

    // ── Computed totals (stored for reporting) ────────────────────────────────

    public decimal Subtotal { get; set; }       // sum of line totals (in invoice currency)
    public decimal DiscountAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }    // Subtotal - Discount + VAT
    public decimal TotalAmountThb { get; set; } // TotalAmount × ExchangeRateToThb
}
