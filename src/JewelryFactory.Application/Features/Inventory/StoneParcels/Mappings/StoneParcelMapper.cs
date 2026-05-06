using JewelryFactory.Application.Features.Inventory.StoneParcels.DTOs;
using JewelryFactory.Domain.Entities;

namespace JewelryFactory.Application.Features.Inventory.StoneParcels.Mappings;

internal static class StoneParcelMapper
{
    public static StoneParcelResponseDto ToResponse(this StoneParcel p) => new(
        p.Id,
        p.ParcelCode,
        p.MaterialTypeId,
        p.MaterialType?.Name ?? "",
        p.TotalCarat,
        p.StoneCount,
        p.AverageSize,
        p.QualityGrade,
        p.Shape.ToString(),
        p.SupplierId,
        p.Supplier?.CompanyName,
        p.ReceivedDate,
        p.Location,
        p.UnitCostPerCarat,
        p.CostCurrency.ToString(),
        p.Status.ToString(),
        p.Notes,
        p.RowVersion,
        p.CreatedAt
    );
}
