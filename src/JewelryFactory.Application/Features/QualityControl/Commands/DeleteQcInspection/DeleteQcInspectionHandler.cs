using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.QualityControl.Commands.DeleteQcInspection;

public class DeleteQcInspectionHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteQcInspectionCommand>
{
    public async Task Handle(DeleteQcInspectionCommand command, CancellationToken ct)
    {
        var inspection = await db.QcInspections
            .FirstOrDefaultAsync(q => q.Id == command.Id && !q.IsDeleted, ct)
            ?? throw new NotFoundException(nameof(QcInspection), command.Id);

        inspection.IsDeleted = true;
        await db.SaveChangesAsync(ct);
    }
}
