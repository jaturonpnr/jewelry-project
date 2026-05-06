using JewelryFactory.Application.Features.Inventory.StoneItems.DTOs;
using JewelryFactory.Domain.Entities;

namespace JewelryFactory.Application.Features.Inventory.StoneItems.Mappings;

internal static class StoneItemMapper
{
    public static StoneItemResponseDto ToResponse(this StoneItem s) => new(
        s.Id,
        s.ItemCode,
        s.MaterialTypeId,
        s.MaterialType?.Name ?? "",
        s.CaratWeight,
        s.Shape.ToString(),
        s.Color?.ToString(),
        s.Clarity?.ToString(),
        s.Cut,
        s.Measurements,
        s.CertAuthority.ToString(),
        s.CertificateNumber,
        s.Origin,
        s.SupplierId,
        s.Supplier?.CompanyName,
        s.ReceivedDate,
        s.Location,
        s.UnitCost,
        s.CostCurrency.ToString(),
        s.Status.ToString(),
        s.Notes,
        s.RowVersion,
        s.CreatedAt
    );
}
