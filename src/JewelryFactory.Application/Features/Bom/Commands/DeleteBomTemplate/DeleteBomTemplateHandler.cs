using JewelryFactory.Domain.Exceptions;
using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Bom.Commands.DeleteBomTemplate;

public class DeleteBomTemplateHandler(IApplicationDbContext db) : IRequestHandler<DeleteBomTemplateCommand>
{
    public async Task Handle(DeleteBomTemplateCommand command, CancellationToken ct)
    {
        var bom = await db.BomTemplates
            .FirstOrDefaultAsync(b => b.Id == command.Id && !b.IsDeleted, ct)
            ?? throw new NotFoundException(nameof(BomTemplate), command.Id);

        bom.IsDeleted = true;
        bom.IsActive = false;
        await db.SaveChangesAsync(ct);
    }
}
