using JewelryFactory.Domain.Exceptions;
using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.Bom.DTOs;
using JewelryFactory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Bom.Queries.GetBomTemplateById;

public class GetBomTemplateByIdHandler(IApplicationDbContext db)
    : IRequestHandler<GetBomTemplateByIdQuery, BomTemplateResponse>
{
    public async Task<BomTemplateResponse> Handle(GetBomTemplateByIdQuery query, CancellationToken ct)
    {
        var bom = await db.BomTemplates
            .AsNoTracking()
            .Include(b => b.MaterialLines)
            .Include(b => b.StoneLines)
            .Include(b => b.LaborLines)
            .FirstOrDefaultAsync(b => b.Id == query.Id && !b.IsDeleted, ct)
            ?? throw new NotFoundException(nameof(BomTemplate), query.Id);

        return BomMapper.ToResponse(bom);
    }
}
