using JewelryFactory.Domain.Common;

namespace JewelryFactory.Domain.Entities;

public class SalesOrderItem : AuditableEntity
{
    public Guid SalesOrderId { get; set; }
    public SalesOrder SalesOrder { get; set; } = null!;

    public int LineNumber { get; set; }
    public required string Description { get; set; }
    public string? DesignCode { get; set; }                 // future link to Design

    /// <summary>
    /// Optional link to a specific FinishedGoods item.
    /// When set, that item is reserved on order Confirm and marked Sold on Ship.
    /// </summary>
    public Guid? FinishedGoodsId { get; set; }
    public FinishedGoods? FinishedGoods { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }                  // in order currency
    public decimal LineDiscount { get; set; }
    public decimal LineTotal { get; set; }                  // (qty × price) - discount
    public string? Notes { get; set; }
}
