using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.Inventory.StoneItems.DTOs;
using JewelryFactory.Application.Features.Inventory.StoneItems.Mappings;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Enums;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Inventory.StoneItems.Commands;

public record ReceiveStoneItemCommand(ReceiveStoneItemDto Request) : IRequest<StoneItemResponseDto>;
public record UpdateStoneItemStatusCommand(Guid Id, UpdateStoneItemStatusDto Request) : IRequest<StoneItemResponseDto>;

public class ReceiveStoneItemHandler(
    IApplicationDbContext db,
    IStockMovementWriter movements
) : IRequestHandler<ReceiveStoneItemCommand, StoneItemResponseDto>
{
    public async Task<StoneItemResponseDto> Handle(ReceiveStoneItemCommand command, CancellationToken ct)
    {
        var dto = command.Request;
        var code = dto.ItemCode.Trim().ToUpperInvariant();

        if (await db.StoneItems.AnyAsync(s => s.ItemCode == code, ct))
            throw new BusinessRuleException($"StoneItem code '{code}' is already in use.");

        var material = await db.MaterialTypes.FirstOrDefaultAsync(m => m.Id == dto.MaterialTypeId, ct)
            ?? throw new NotFoundException(nameof(MaterialType), dto.MaterialTypeId);

        // Business rule (CLAUDE.md §10.4): individual tracking is for stones ≥ 0.20 ct OR with cert.
        var hasCert = dto.CertAuthority != StoneCertAuthority.None
                      && !string.IsNullOrWhiteSpace(dto.CertificateNumber);
        if (dto.CaratWeight < 0.20m && !hasCert)
            throw new BusinessRuleException(
                "Stones below 0.20 carat without a certificate must be tracked as a parcel, not an item.");

        var item = new StoneItem
        {
            ItemCode = code,
            MaterialTypeId = material.Id,
            CaratWeight = dto.CaratWeight,
            Shape = dto.Shape,
            Color = dto.Color,
            Clarity = dto.Clarity,
            Cut = dto.Cut?.Trim(),
            Measurements = dto.Measurements?.Trim(),
            CertAuthority = dto.CertAuthority,
            CertificateNumber = dto.CertificateNumber?.Trim(),
            Origin = dto.Origin?.Trim(),
            SupplierId = dto.SupplierId,
            ReceivedDate = dto.ReceivedDate,
            Location = dto.Location.Trim().ToUpperInvariant(),
            UnitCost = dto.UnitCost,
            CostCurrency = dto.CostCurrency,
            Status = StockStatus.InStock,
            Notes = dto.Notes?.Trim()
        };

        db.StoneItems.Add(item);

        movements.Record(
            InventoryItemType.StoneItem,
            item.Id,
            StockMovementType.Receipt,
            quantityDelta: dto.CaratWeight,
            quantityAfter: dto.CaratWeight,
            reason: "Initial receipt");

        await db.SaveChangesAsync(ct);

        var saved = await db.StoneItems
            .Include(x => x.MaterialType).Include(x => x.Supplier)
            .FirstAsync(x => x.Id == item.Id, ct);
        return saved.ToResponse();
    }
}

public class UpdateStoneItemStatusHandler(
    IApplicationDbContext db,
    IStockMovementWriter movements
) : IRequestHandler<UpdateStoneItemStatusCommand, StoneItemResponseDto>
{
    public async Task<StoneItemResponseDto> Handle(UpdateStoneItemStatusCommand command, CancellationToken ct)
    {
        var item = await db.StoneItems
            .Include(x => x.MaterialType).Include(x => x.Supplier)
            .FirstOrDefaultAsync(x => x.Id == command.Id, ct)
            ?? throw new NotFoundException(nameof(StoneItem), command.Id);

        var dto = command.Request;
        if (item.Status == dto.Status)
            return item.ToResponse();

        var oldStatus = item.Status;
        item.Status = dto.Status;
        if (!string.IsNullOrWhiteSpace(dto.Notes))
            item.Notes = dto.Notes.Trim();

        db.Entry(item).Property(x => x.RowVersion).OriginalValue = dto.RowVersion;

        var movementType = dto.Status switch
        {
            StockStatus.Reserved => StockMovementType.Reserve,
            StockStatus.InStock when oldStatus == StockStatus.Reserved => StockMovementType.Release,
            StockStatus.InProduction => StockMovementType.Issue,
            StockStatus.WrittenOff => StockMovementType.WriteOff,
            StockStatus.Returned => StockMovementType.Return,
            _ => StockMovementType.Adjustment
        };

        movements.Record(
            InventoryItemType.StoneItem,
            item.Id,
            movementType,
            quantityDelta: 0,
            quantityAfter: item.CaratWeight,
            reason: $"Status: {oldStatus} → {dto.Status}",
            notes: dto.Notes);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException(nameof(StoneItem), command.Id);
        }

        return item.ToResponse();
    }
}
