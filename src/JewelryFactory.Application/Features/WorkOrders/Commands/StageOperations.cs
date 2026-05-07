using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.WorkOrders.DTOs;
using JewelryFactory.Application.Features.WorkOrders.Mappings;
using JewelryFactory.Domain.Entities;
using JewelryFactory.Domain.Enums;
using JewelryFactory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.WorkOrders.Commands;

// ── Commands ─────────────────────────────────────────────────────────────

public record StartStageCommand(Guid WorkOrderId, Guid StageId, StartStageDto Request)
    : IRequest<WorkOrderStageResponseDto>;

public record CompleteStageCommand(Guid WorkOrderId, Guid StageId, CompleteStageDto Request)
    : IRequest<WorkOrderStageResponseDto>;

public record SkipStageCommand(Guid WorkOrderId, Guid StageId, SkipStageDto Request)
    : IRequest<WorkOrderStageResponseDto>;

public record FailStageCommand(Guid WorkOrderId, Guid StageId, FailStageDto Request)
    : IRequest<WorkOrderStageResponseDto>;

// ── Handlers ─────────────────────────────────────────────────────────────

public class StartStageHandler(IApplicationDbContext db)
    : IRequestHandler<StartStageCommand, WorkOrderStageResponseDto>
{
    public async Task<WorkOrderStageResponseDto> Handle(StartStageCommand command, CancellationToken ct)
    {
        var (wo, stage) = await StageLoader.LoadAsync(db, command.WorkOrderId, command.StageId, ct);

        if (wo.Status != WorkOrderStatus.Released && wo.Status != WorkOrderStatus.InProgress)
            throw new BusinessRuleException(
                $"Stages can only be started when the work order is Released or InProgress (current: {wo.Status}).");

        if (stage.Status != WorkOrderStageStatus.Pending)
            throw new BusinessRuleException(
                $"Only Pending stages can be started (current: {stage.Status}).");

        var dto = command.Request;
        stage.Status = WorkOrderStageStatus.InProgress;
        stage.StartedAt = DateTime.UtcNow;
        stage.AssignedWorkerId = dto.AssignedWorkerId ?? stage.AssignedWorkerId;
        stage.WeightInGrams = dto.WeightInGrams;

        // Move WO to InProgress on first stage start
        if (wo.Status == WorkOrderStatus.Released)
        {
            wo.Status = WorkOrderStatus.InProgress;
            wo.ActualStart ??= DateTime.UtcNow;
        }

        db.Entry(stage).Property(x => x.RowVersion).OriginalValue = dto.RowVersion;

        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException(nameof(WorkOrderStage), command.StageId);
        }

        return stage.ToResponse();
    }
}

public class CompleteStageHandler(IApplicationDbContext db)
    : IRequestHandler<CompleteStageCommand, WorkOrderStageResponseDto>
{
    public async Task<WorkOrderStageResponseDto> Handle(CompleteStageCommand command, CancellationToken ct)
    {
        var (_, stage) = await StageLoader.LoadAsync(db, command.WorkOrderId, command.StageId, ct);

        if (stage.Status != WorkOrderStageStatus.InProgress)
            throw new BusinessRuleException(
                $"Only InProgress stages can be completed (current: {stage.Status}).");

        var dto = command.Request;

        // Weight reconciliation guard (CLAUDE.md §10.6).
        // Negative loss (WeightOut > WeightIn) is allowed and surfaces via the
        // computed LossGrams field — operators may flag it later as a data anomaly.
        if (dto.WeightOutGrams.HasValue && dto.WeightOutGrams.Value < 0)
            throw new BusinessRuleException("Weight out cannot be negative.");

        stage.Status = WorkOrderStageStatus.Completed;
        stage.CompletedAt = DateTime.UtcNow;
        stage.WeightOutGrams = dto.WeightOutGrams;
        stage.ActualHours = dto.ActualHours;
        stage.Notes = dto.Notes?.Trim();

        db.Entry(stage).Property(x => x.RowVersion).OriginalValue = dto.RowVersion;

        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException(nameof(WorkOrderStage), command.StageId);
        }

        return stage.ToResponse();
    }
}

public class SkipStageHandler(IApplicationDbContext db)
    : IRequestHandler<SkipStageCommand, WorkOrderStageResponseDto>
{
    public async Task<WorkOrderStageResponseDto> Handle(SkipStageCommand command, CancellationToken ct)
    {
        var (_, stage) = await StageLoader.LoadAsync(db, command.WorkOrderId, command.StageId, ct);

        if (stage.Status != WorkOrderStageStatus.Pending)
            throw new BusinessRuleException(
                $"Only Pending stages can be skipped (current: {stage.Status}).");

        var dto = command.Request;
        stage.Status = WorkOrderStageStatus.Skipped;
        stage.Notes = $"Skipped: {dto.Reason.Trim()}";

        db.Entry(stage).Property(x => x.RowVersion).OriginalValue = dto.RowVersion;

        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException(nameof(WorkOrderStage), command.StageId);
        }

        return stage.ToResponse();
    }
}

public class FailStageHandler(IApplicationDbContext db)
    : IRequestHandler<FailStageCommand, WorkOrderStageResponseDto>
{
    public async Task<WorkOrderStageResponseDto> Handle(FailStageCommand command, CancellationToken ct)
    {
        var (_, stage) = await StageLoader.LoadAsync(db, command.WorkOrderId, command.StageId, ct);

        if (stage.Status != WorkOrderStageStatus.InProgress)
            throw new BusinessRuleException(
                $"Only InProgress stages can be marked Failed (current: {stage.Status}).");

        var dto = command.Request;
        stage.Status = WorkOrderStageStatus.Failed;
        stage.CompletedAt = DateTime.UtcNow;
        stage.WeightOutGrams = dto.WeightOutGrams;
        stage.FailureReason = dto.FailureReason.Trim();

        db.Entry(stage).Property(x => x.RowVersion).OriginalValue = dto.RowVersion;

        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException(nameof(WorkOrderStage), command.StageId);
        }

        return stage.ToResponse();
    }
}

// ── Shared loader ────────────────────────────────────────────────────────

internal static class StageLoader
{
    public static async Task<(WorkOrder Wo, WorkOrderStage Stage)> LoadAsync(
        IApplicationDbContext db, Guid woId, Guid stageId, CancellationToken ct)
    {
        var wo = await db.WorkOrders
            .Include(w => w.Stages).ThenInclude(s => s.AssignedWorker)
            .FirstOrDefaultAsync(w => w.Id == woId, ct)
            ?? throw new NotFoundException(nameof(WorkOrder), woId);

        var stage = wo.Stages.FirstOrDefault(s => s.Id == stageId)
            ?? throw new NotFoundException(nameof(WorkOrderStage), stageId);

        return (wo, stage);
    }
}
