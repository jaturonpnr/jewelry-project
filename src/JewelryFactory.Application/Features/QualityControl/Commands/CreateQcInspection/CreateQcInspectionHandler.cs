using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.QualityControl.Commands.CreateQcInspection;

public class CreateQcInspectionHandler(IApplicationDbContext db)
    : IRequestHandler<CreateQcInspectionCommand, Guid>
{
    public async Task<Guid> Handle(CreateQcInspectionCommand command, CancellationToken ct)
    {
        var req = command.Request;

        var inspection = new QcInspection
        {
            InspectionType = req.InspectionType,
            InspectionDate = req.InspectionDate.ToUniversalTime(),
            WorkOrderId = req.WorkOrderId,
            WorkOrderStageId = req.WorkOrderStageId,
            RawMaterialItemId = req.RawMaterialItemId,
            InspectorId = req.InspectorId,
            InspectorName = req.InspectorName.Trim(),
            Result = req.Result,
            Notes = req.Notes?.Trim(),
            ReworkStage = req.ReworkStage,
            ActualWeightGrams = req.ActualWeightGrams,
            ExpectedWeightGrams = req.ExpectedWeightGrams,
        };

        foreach (var d in req.Defects)
        {
            inspection.Defects.Add(new QcDefect
            {
                DefectType = d.DefectType.Trim(),
                Severity = d.Severity,
                Description = d.Description?.Trim(),
                Quantity = d.Quantity,
            });
        }

        db.QcInspections.Add(inspection);

        // If In-Process QC fails → mark the WO stage as Failed
        if (req.Result == QcResult.Fail && req.WorkOrderStageId.HasValue)
        {
            var stage = await db.WorkOrderStages
                .FirstOrDefaultAsync(s => s.Id == req.WorkOrderStageId.Value, ct);
            if (stage is not null)
                stage.Status = WorkOrderStageStatus.Failed;
        }

        await db.SaveChangesAsync(ct);
        return inspection.Id;
    }
}
