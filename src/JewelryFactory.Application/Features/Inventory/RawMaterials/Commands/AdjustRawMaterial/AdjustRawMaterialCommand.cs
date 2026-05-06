using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.Inventory.RawMaterials.DTOs;
using JewelryFactory.Application.Features.Inventory.RawMaterials.Mappings;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Enums;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Inventory.RawMaterials.Commands.AdjustRawMaterial;

/// <summary>
/// Adjusts the quantity of a raw material item.
/// Uses optimistic concurrency (RowVersion) per CLAUDE.md §12.7
/// to prevent double-deduction by concurrent users.
/// </summary>
public record AdjustRawMaterialCommand(Guid Id, AdjustRawMaterialDto Request) : IRequest<RawMaterialResponseDto>;

public class AdjustRawMaterialHandler(
    IApplicationDbContext db,
    IStockMovementWriter movements
) : IRequestHandler<AdjustRawMaterialCommand, RawMaterialResponseDto>
{
    public async Task<RawMaterialResponseDto> Handle(AdjustRawMaterialCommand command, CancellationToken ct)
    {
        var item = await db.RawMaterialItems
            .Include(x => x.MaterialType)
            .Include(x => x.Supplier)
            .FirstOrDefaultAsync(x => x.Id == command.Id, ct)
            ?? throw new NotFoundException(nameof(RawMaterialItem), command.Id);

        var dto = command.Request;
        var newQuantity = item.Quantity + dto.QuantityDelta;

        if (newQuantity < 0)
            throw new BusinessRuleException(
                $"Adjustment would make quantity negative (current {item.Quantity}, delta {dto.QuantityDelta}).");

        item.Quantity = newQuantity;

        // Recompute pure weight for metals (CLAUDE.md §10.3)
        if (item.MaterialType.Category == MaterialCategory.Metal
            && item.MaterialType.PurityFraction.HasValue)
        {
            item.PureWeight = Math.Round(newQuantity * item.MaterialType.PurityFraction.Value, 4);
        }

        // Inform EF of the client's expected RowVersion so the concurrency check fires.
        db.Entry(item).Property(x => x.RowVersion).OriginalValue = dto.RowVersion;

        var movementType = dto.QuantityDelta >= 0 ? StockMovementType.Adjustment : StockMovementType.Issue;
        movements.Record(
            InventoryItemType.RawMaterial,
            item.Id,
            movementType,
            quantityDelta: dto.QuantityDelta,
            quantityAfter: newQuantity,
            reason: dto.Reason,
            notes: dto.Notes);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException(nameof(RawMaterialItem), command.Id);
        }

        return item.ToResponse();
    }
}
