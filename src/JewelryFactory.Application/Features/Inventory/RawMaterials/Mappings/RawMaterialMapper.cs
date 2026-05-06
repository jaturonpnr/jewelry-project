using JewelryFactory.Application.Features.Inventory.RawMaterials.DTOs;
using JewelryFactory.Domain.Entities;

namespace JewelryFactory.Application.Features.Inventory.RawMaterials.Mappings;

internal static class RawMaterialMapper
{
    public static RawMaterialResponseDto ToResponse(this RawMaterialItem r) => new(
        r.Id,
        r.MaterialTypeId,
        r.MaterialType?.Code ?? "",
        r.MaterialType?.Name ?? "",
        r.MaterialType?.Unit.ToString() ?? "",
        r.LotNumber,
        r.Location,
        r.Quantity,
        r.PureWeight,
        r.UnitCost,
        r.CostCurrency.ToString(),
        r.SupplierId,
        r.Supplier?.CompanyName,
        r.ReceivedDate,
        r.Status.ToString(),
        r.Notes,
        r.RowVersion,
        r.CreatedAt
    );
}
