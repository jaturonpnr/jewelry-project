using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.QualityControl.DTOs;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.QualityControl.Queries.GetQcInspectionById;

public class GetQcInspectionByIdHandler(IApplicationDbContext db)
    : IRequestHandler<GetQcInspectionByIdQuery, QcInspectionResponse>
{
    public async Task<QcInspectionResponse> Handle(
        GetQcInspectionByIdQuery query, CancellationToken ct)
    {
        var x = await db.QcInspections
            .AsNoTracking()
            .Include(q => q.WorkOrder)
            .Include(q => q.WorkOrderStage)
            .Include(q => q.RawMaterialItem)
            .Include(q => q.Defects)
            .FirstOrDefaultAsync(q => q.Id == query.Id && !q.IsDeleted, ct)
            ?? throw new NotFoundException(nameof(QcInspection), query.Id);

        return new QcInspectionResponse(
            x.Id,
            x.InspectionType,
            x.InspectionDate,
            x.WorkOrderId,
            x.WorkOrder?.WorkOrderNumber,
            x.WorkOrderStageId,
            x.WorkOrderStage?.Stage.ToString(),
            x.RawMaterialItemId,
            x.RawMaterialItem?.LotNumber,
            x.InspectorId,
            x.InspectorName,
            x.Result,
            x.Notes,
            x.ReworkStage,
            x.ActualWeightGrams,
            x.ExpectedWeightGrams,
            x.Defects.Count,
            x.Defects.Select(d => new QcDefectResponse(
                d.Id, d.DefectType, d.Severity, d.Description, d.Quantity))
            .ToList(),
            x.CreatedAt);
    }
}
