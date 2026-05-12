using JewelryFactory.Domain.Exceptions;
using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Bom.Commands.UpdateBomTemplate;

public class UpdateBomTemplateHandler(IApplicationDbContext db) : IRequestHandler<UpdateBomTemplateCommand>
{
    public async Task Handle(UpdateBomTemplateCommand command, CancellationToken ct)
    {
        var bom = await db.BomTemplates
            .Include(b => b.MaterialLines)
            .Include(b => b.StoneLines)
            .Include(b => b.LaborLines)
            .FirstOrDefaultAsync(b => b.Id == command.Id && !b.IsDeleted, ct)
            ?? throw new NotFoundException(nameof(BomTemplate), command.Id);

        var req = command.Request;

        bom.DesignCode = req.DesignCode.Trim().ToUpper();
        bom.DesignName = req.DesignName.Trim();
        bom.Description = req.Description?.Trim();
        bom.OverheadPercent = req.OverheadPercent;
        bom.IsActive = req.IsActive;

        // Replace all lines (simpler than partial diff for BOM editing)
        db.BomMaterialLines.RemoveRange(bom.MaterialLines);
        db.BomStoneLines.RemoveRange(bom.StoneLines);
        db.BomLaborLines.RemoveRange(bom.LaborLines);

        bom.MaterialLines = req.MaterialLines.Select(m => new BomMaterialLine
        {
            BomTemplateId = bom.Id,
            Category = m.Category,
            MaterialDescription = m.MaterialDescription.Trim(),
            Karat = m.Karat,
            PurityFraction = m.PurityFraction,
            QuantityGrams = m.QuantityGrams,
            ExpectedLossPercent = m.ExpectedLossPercent,
            UnitCostThbPerGram = m.UnitCostThbPerGram,
            SortOrder = m.SortOrder,
        }).ToList();

        bom.StoneLines = req.StoneLines.Select(s => new BomStoneLine
        {
            BomTemplateId = bom.Id,
            StoneType = s.StoneType.Trim(),
            StoneShape = s.StoneShape.Trim(),
            SizeDescription = s.SizeDescription.Trim(),
            CaratPerStone = s.CaratPerStone,
            Quantity = s.Quantity,
            TrackingType = s.TrackingType,
            UnitCostThbPerCarat = s.UnitCostThbPerCarat,
            SortOrder = s.SortOrder,
        }).ToList();

        bom.LaborLines = req.LaborLines.Select(l => new BomLaborLine
        {
            BomTemplateId = bom.Id,
            Stage = l.Stage,
            EstimatedHours = l.EstimatedHours,
            HourlyRateThb = l.HourlyRateThb,
        }).ToList();

        await db.SaveChangesAsync(ct);
    }
}
