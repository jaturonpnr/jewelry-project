using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.MaterialTypes.DTOs;
using JewelryFactory.Application.Features.MaterialTypes.Mappings;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Enums;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.MaterialTypes.Commands;

public record CreateMaterialTypeCommand(CreateMaterialTypeDto Request) : IRequest<MaterialTypeResponseDto>;
public record UpdateMaterialTypeCommand(Guid Id, UpdateMaterialTypeDto Request) : IRequest<MaterialTypeResponseDto>;
public record DeleteMaterialTypeCommand(Guid Id) : IRequest;

public class CreateMaterialTypeHandler(IApplicationDbContext db)
    : IRequestHandler<CreateMaterialTypeCommand, MaterialTypeResponseDto>
{
    public async Task<MaterialTypeResponseDto> Handle(CreateMaterialTypeCommand command, CancellationToken ct)
    {
        var dto = command.Request;
        var code = dto.Code.Trim().ToUpperInvariant();

        if (await db.MaterialTypes.AnyAsync(m => m.Code == code, ct))
            throw new BusinessRuleException($"MaterialType code '{code}' is already in use.");

        // Business rule: metals must have purity + karat (CLAUDE.md §10.2-3)
        if (dto.Category == MaterialCategory.Metal && (dto.PurityFraction is null || dto.Karat is null))
            throw new BusinessRuleException("Metal material types must include both PurityFraction and Karat.");

        var entity = new MaterialType
        {
            Code = code,
            Name = dto.Name.Trim(),
            Category = dto.Category,
            Unit = dto.Unit,
            PurityFraction = dto.PurityFraction,
            Karat = dto.Karat,
            Description = dto.Description?.Trim()
        };

        db.MaterialTypes.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.ToResponse();
    }
}

public class UpdateMaterialTypeHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateMaterialTypeCommand, MaterialTypeResponseDto>
{
    public async Task<MaterialTypeResponseDto> Handle(UpdateMaterialTypeCommand command, CancellationToken ct)
    {
        var entity = await db.MaterialTypes.FirstOrDefaultAsync(m => m.Id == command.Id, ct)
            ?? throw new NotFoundException(nameof(MaterialType), command.Id);

        var dto = command.Request;
        if (dto.Category == MaterialCategory.Metal && (dto.PurityFraction is null || dto.Karat is null))
            throw new BusinessRuleException("Metal material types must include both PurityFraction and Karat.");

        entity.Name = dto.Name.Trim();
        entity.Category = dto.Category;
        entity.Unit = dto.Unit;
        entity.PurityFraction = dto.PurityFraction;
        entity.Karat = dto.Karat;
        entity.IsActive = dto.IsActive;
        entity.Description = dto.Description?.Trim();

        await db.SaveChangesAsync(ct);
        return entity.ToResponse();
    }
}

public class DeleteMaterialTypeHandler(IApplicationDbContext db) : IRequestHandler<DeleteMaterialTypeCommand>
{
    public async Task Handle(DeleteMaterialTypeCommand command, CancellationToken ct)
    {
        var entity = await db.MaterialTypes.FirstOrDefaultAsync(m => m.Id == command.Id, ct)
            ?? throw new NotFoundException(nameof(MaterialType), command.Id);
        db.MaterialTypes.Remove(entity);
        await db.SaveChangesAsync(ct);
    }
}
