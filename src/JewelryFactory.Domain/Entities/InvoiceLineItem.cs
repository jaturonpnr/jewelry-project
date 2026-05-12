using JewelryFactory.Domain.Common;

namespace JewelryFactory.Domain.Entities;

public class InvoiceLineItem : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;

    public int LineNumber { get; set; }
    public required string Description { get; set; }
    public string? DesignCode { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }        // in invoice currency
    public decimal DiscountPercent { get; set; }
    public decimal LineTotal { get; set; }         // qty × price × (1 - discount/100)
}
