using JewelryFactory.Application.Features.Bom.DTOs;
using JewelryFactory.Domain.Entities;

namespace JewelryFactory.Application.Features.Bom;

internal static class BomMapper
{
    internal static BomTemplateResponse ToResponse(BomTemplate bom)
    {
        var materialLines = bom.MaterialLines
            .OrderBy(m => m.SortOrder)
            .Select(m => new BomMaterialLineResponse(
                m.Id,
                m.Category,
                m.MaterialDescription,
                m.Karat,
                m.PurityFraction,
                m.QuantityGrams,
                m.ExpectedLossPercent,
                m.UnitCostThbPerGram,
                m.QuantityGrams * m.UnitCostThbPerGram,
                m.SortOrder))
            .ToList();

        var stoneLines = bom.StoneLines
            .OrderBy(s => s.SortOrder)
            .Select(s => new BomStoneLineResponse(
                s.Id,
                s.StoneType,
                s.StoneShape,
                s.SizeDescription,
                s.CaratPerStone,
                s.Quantity,
                s.TotalCaratWeight,
                s.TrackingType,
                s.UnitCostThbPerCarat,
                s.LineCostThb,
                s.SortOrder))
            .ToList();

        var laborLines = bom.LaborLines
            .OrderBy(l => l.Stage)
            .Select(l => new BomLaborLineResponse(
                l.Id,
                l.Stage,
                l.Stage.ToString(),
                l.EstimatedHours,
                l.HourlyRateThb,
                l.LaborCostThb))
            .ToList();

        var materialCost = materialLines.Sum(m => m.LineCostThb);
        var stoneCost = stoneLines.Sum(s => s.LineCostThb);
        var laborCost = laborLines.Sum(l => l.LaborCostThb);
        var subtotal = materialCost + stoneCost + laborCost;
        var overheadCost = subtotal * (bom.OverheadPercent / 100m);
        var total = subtotal + overheadCost;

        var costSummary = new BomCostSummary(
            materialCost, stoneCost, laborCost, subtotal,
            bom.OverheadPercent, overheadCost, total);

        return new BomTemplateResponse(
            bom.Id, bom.DesignCode, bom.DesignName, bom.Description,
            bom.IsActive, bom.OverheadPercent,
            materialLines, stoneLines, laborLines, costSummary,
            bom.CreatedAt, bom.UpdatedAt);
    }

    internal static BomTemplateSummaryResponse ToSummary(BomTemplate bom)
    {
        var materialCost = bom.MaterialLines.Sum(m => m.QuantityGrams * m.UnitCostThbPerGram);
        var stoneCost = bom.StoneLines.Sum(s => s.TotalCaratWeight * s.UnitCostThbPerCarat);
        var laborCost = bom.LaborLines.Sum(l => l.LaborCostThb);
        var subtotal = materialCost + stoneCost + laborCost;
        var total = subtotal * (1 + bom.OverheadPercent / 100m);

        return new BomTemplateSummaryResponse(
            bom.Id, bom.DesignCode, bom.DesignName, bom.IsActive,
            bom.MaterialLines.Count, bom.StoneLines.Count, bom.LaborLines.Count,
            total, bom.CreatedAt);
    }
}
