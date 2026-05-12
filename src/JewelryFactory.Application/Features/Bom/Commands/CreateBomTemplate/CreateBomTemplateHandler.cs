using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Domain.Entities;
using MediatR;

namespace JewelryFactory.Application.Features.Bom.Commands.CreateBomTemplate;

public class CreateBomTemplateHandler(IApplicationDbContext db) : IRequestHandler<CreateBomTemplateCommand, Guid>
{
    public async Task<Guid> Handle(CreateBomTemplateCommand command, CancellationToken ct)
    {
        var req = command.Request;

        var bom = new BomTemplate
        {
            DesignCode = req.DesignCode.Trim().ToUpper(),
            DesignName = req.DesignName.Trim(),
            Description = req.Description?.Trim(),
            OverheadPercent = req.OverheadPercent,
        };

        foreach (var m in req.MaterialLines)
        {
            bom.MaterialLines.Add(new BomMaterialLine
            {
                Category = m.Category,
                MaterialDescription = m.MaterialDescription.Trim(),
                Karat = m.Karat,
                PurityFraction = m.PurityFraction,
                QuantityGrams = m.QuantityGrams,
                ExpectedLossPercent = m.ExpectedLossPercent,
                UnitCostThbPerGram = m.UnitCostThbPerGram,
                SortOrder = m.SortOrder,
            });
        }

        foreach (var s in req.StoneLines)
        {
            bom.StoneLines.Add(new BomStoneLine
            {
                StoneType = s.StoneType.Trim(),
                StoneShape = s.StoneShape.Trim(),
                SizeDescription = s.SizeDescription.Trim(),
                CaratPerStone = s.CaratPerStone,
                Quantity = s.Quantity,
                TrackingType = s.TrackingType,
                UnitCostThbPerCarat = s.UnitCostThbPerCarat,
                SortOrder = s.SortOrder,
            });
        }

        foreach (var l in req.LaborLines)
        {
            bom.LaborLines.Add(new BomLaborLine
            {
                Stage = l.Stage,
                EstimatedHours = l.EstimatedHours,
                HourlyRateThb = l.HourlyRateThb,
            });
        }

        db.BomTemplates.Add(bom);
        await db.SaveChangesAsync(ct);
        return bom.Id;
    }
}
