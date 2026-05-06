using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.Inventory.StoneParcels.DTOs;
using JewelryFactory.Application.Features.Inventory.StoneParcels.Mappings;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Enums;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Inventory.StoneParcels.Commands;

public record ReceiveStoneParcelCommand(ReceiveStoneParcelDto Request) : IRequest<StoneParcelResponseDto>;
public record AdjustStoneParcelCommand(Guid Id, AdjustStoneParcelDto Request) : IRequest<StoneParcelResponseDto>;

public class ReceiveStoneParcelHandler(
    IApplicationDbContext db,
    IStockMovementWriter movements
) : IRequestHandler<ReceiveStoneParcelCommand, StoneParcelResponseDto>
{
    public async Task<StoneParcelResponseDto> Handle(ReceiveStoneParcelCommand command, CancellationToken ct)
    {
        var dto = command.Request;
        var code = dto.ParcelCode.Trim().ToUpperInvariant();

        if (await db.StoneParcels.AnyAsync(p => p.ParcelCode == code, ct))
            throw new BusinessRuleException($"Parcel code '{code}' is already in use.");

        var avg = dto.StoneCount > 0
            ? Math.Round(dto.TotalCarat / dto.StoneCount, 4)
            : 0m;

        // Sanity: parcel is intended for stones < 0.20 ct (CLAUDE.md §10.4)
        if (avg >= 0.20m)
            throw new BusinessRuleException(
                $"Average stone size {avg:F4} ct ≥ 0.20 ct. Track these as individual StoneItems instead.");

        var parcel = new StoneParcel
        {
            ParcelCode = code,
            MaterialTypeId = dto.MaterialTypeId,
            TotalCarat = dto.TotalCarat,
            StoneCount = dto.StoneCount,
            AverageSize = avg,
            QualityGrade = dto.QualityGrade?.Trim(),
            Shape = dto.Shape,
            SupplierId = dto.SupplierId,
            ReceivedDate = dto.ReceivedDate,
            Location = dto.Location.Trim().ToUpperInvariant(),
            UnitCostPerCarat = dto.UnitCostPerCarat,
            CostCurrency = dto.CostCurrency,
            Status = StockStatus.InStock,
            Notes = dto.Notes?.Trim()
        };

        db.StoneParcels.Add(parcel);

        movements.Record(
            InventoryItemType.StoneParcel,
            parcel.Id,
            StockMovementType.Receipt,
            quantityDelta: dto.TotalCarat,
            quantityAfter: dto.TotalCarat,
            reason: "Initial receipt");

        await db.SaveChangesAsync(ct);

        var saved = await db.StoneParcels
            .Include(x => x.MaterialType).Include(x => x.Supplier)
            .FirstAsync(x => x.Id == parcel.Id, ct);
        return saved.ToResponse();
    }
}

public class AdjustStoneParcelHandler(
    IApplicationDbContext db,
    IStockMovementWriter movements
) : IRequestHandler<AdjustStoneParcelCommand, StoneParcelResponseDto>
{
    public async Task<StoneParcelResponseDto> Handle(AdjustStoneParcelCommand command, CancellationToken ct)
    {
        var parcel = await db.StoneParcels
            .Include(x => x.MaterialType).Include(x => x.Supplier)
            .FirstOrDefaultAsync(x => x.Id == command.Id, ct)
            ?? throw new NotFoundException(nameof(StoneParcel), command.Id);

        var dto = command.Request;
        var newCarat = parcel.TotalCarat + dto.CaratDelta;
        var newCount = parcel.StoneCount + dto.StoneCountDelta;

        if (newCarat < 0 || newCount < 0)
            throw new BusinessRuleException("Adjustment would make TotalCarat or StoneCount negative.");

        parcel.TotalCarat = newCarat;
        parcel.StoneCount = newCount;
        parcel.AverageSize = newCount > 0 ? Math.Round(newCarat / newCount, 4) : 0m;

        db.Entry(parcel).Property(x => x.RowVersion).OriginalValue = dto.RowVersion;

        var movementType = dto.CaratDelta >= 0 ? StockMovementType.Adjustment : StockMovementType.Issue;
        movements.Record(
            InventoryItemType.StoneParcel,
            parcel.Id,
            movementType,
            quantityDelta: dto.CaratDelta,
            quantityAfter: newCarat,
            reason: dto.Reason,
            notes: dto.Notes);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException(nameof(StoneParcel), command.Id);
        }

        return parcel.ToResponse();
    }
}
