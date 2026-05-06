namespace JewelryFactory.Domain.ValueObjects;

/// <summary>
/// Address value object — owned entity in EF Core.
/// </summary>
public class Address
{
    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
}
