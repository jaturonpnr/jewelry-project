using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.Inventory.RawMaterials.DTOs;
using JewelryFactory.Application.Features.Inventory.RawMaterials.Mappings;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Enums;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Inventory.RawMaterials.Commands.ReceiveRawMaterial;

public record ReceiveRawMaterialCommand(ReceiveRawMaterialDto Request) : IRequest<RawMaterialResponseDto>;

public class ReceiveRawMaterialHandler(
    IApplicationDbContext db,
    IStockMovementWriter movements
) : IRequestHandler<ReceiveRawMaterialCommand, RawMaterialResponseDto>
{
    public async Task<RawMaterialResponseDto> Handle(ReceiveRawMaterialCommand command, CancellationToken ct)
    {
        var dto = command.Request;

        var material = await db.MaterialTypes.FirstOrDefaultAsync(m => m.Id == dto.MaterialTypeId, ct)
            ?? throw new NotFoundException(nameof(MaterialType), dto.MaterialTypeId);

        if (!material.IsActive)
            throw new BusinessRuleException("Cannot receive stock for an inactive material type.");

        // Compute pure weight for metals (CLAUDE.md §10.3)
        decimal? pureWeight = null;
        if (material.Category == MaterialCategory.Metal && material.PurityFraction.HasValue)
            pureWeight = Math.Round(dto.Quantity * material.PurityFraction.Value, 4);

        var item = new RawMaterialItem
        {
            MaterialTypeId = material.Id,
            LotNumber = dto.LotNumber.Trim().ToUpperInvariant(),
            Location = dto.Location.Trim().ToUpperInvariant(),
            Quantity = dto.Quantity,
            PureWeight = pureWeight,
            UnitCost = dto.UnitCost,
            CostCurrency = dto.CostCurrency,
            SupplierId = dto.SupplierId,
            ReceivedDate = dto.ReceivedDate,
            Status = StockStatus.InStock,
            Notes = dto.Notes?.Trim()
        };

        db.RawMaterialItems.Add(item);

        movements.Record(
            InventoryItemType.RawMaterial,
            item.Id,
            StockMovementType.Receipt,
            quantityDelta: dto.Quantity,
            quantityAfter: dto.Quantity,
            reason: "Initial receipt");

        await db.SaveChangesAsync(ct);

        // Re-load with includes for the response
        var saved = await db.RawMaterialItems
            .Include(x => x.MaterialType)
            .Include(x => x.Supplier)
            .FirstAsync(x => x.Id == item.Id, ct);

        return saved.ToResponse();
    }
}
