using JewelryFactory.Application.Common.Interfaces;
using JewelryFactory.Application.Features.Reports.DTOs;
using JewelryFactory.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryFactory.Application.Features.Reports.Queries;

public record GetProductionLoadReportQuery() : IRequest<ProductionLoadReportDto>;

public class GetProductionLoadReportHandler(IApplicationDbContext db)
    : IRequestHandler<GetProductionLoadReportQuery, ProductionLoadReportDto>
{
    public async Task<ProductionLoadReportDto> Handle(GetProductionLoadReportQuery query, CancellationToken ct)
    {
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // ── WO counts ─────────────────────────────────────────────────────────────
        var activeCount = await db.WorkOrders.AsNoTracking()
            .CountAsync(w => w.Status == WorkOrderStatus.Released
                          || w.Status == WorkOrderStatus.InProgress, ct);

        var completedThisMonth = await db.WorkOrders.AsNoTracking()
            .CountAsync(w => w.Status == WorkOrderStatus.Completed
                          && w.CompletedAt.HasValue
                          && w.CompletedAt.Value >= monthStart, ct);

        var cancelledThisMonth = await db.WorkOrders.AsNoTracking()
            .CountAsync(w => w.Status == WorkOrderStatus.Cancelled
                          && w.CancelledAt.HasValue
                          && w.CancelledAt.Value >= monthStart, ct);

        // ── Loss this month — sum of (WeightIn - WeightOut) for stages completed in current month ─
        var stagesThisMonth = await db.WorkOrderStages.AsNoTracking()
            .Where(s => s.Status == WorkOrderStageStatus.Completed
                     && s.CompletedAt.HasValue
                     && s.CompletedAt.Value >= monthStart
                     && s.WeightInGrams.HasValue
                     && s.WeightOutGrams.HasValue)
            .Select(s => new { s.WeightInGrams, s.WeightOutGrams })
            .ToListAsync(ct);
        var totalLossGrams = stagesThisMonth
            .Sum(s => (s.WeightInGrams ?? 0) - (s.WeightOutGrams ?? 0));

        // ── WOs by status ─────────────────────────────────────────────────────────
        var byStatusRaw = await db.WorkOrders.AsNoTracking()
            .GroupBy(w => w.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);
        var byStatus = byStatusRaw
            .Select(x => new WorkOrdersByStatusDto(x.Status.ToString(), x.Count))
            .OrderBy(x => x.Status)
            .ToList();

        // ── WOs by priority ───────────────────────────────────────────────────────
        var byPriorityRaw = await db.WorkOrders.AsNoTracking()
            .Where(w => w.Status == WorkOrderStatus.Released
                     || w.Status == WorkOrderStatus.InProgress)
            .GroupBy(w => w.Priority)
            .Select(g => new { Priority = g.Key, Count = g.Count() })
            .ToListAsync(ct);
        var byPriority = byPriorityRaw
            .Select(x => new WorkOrdersByPriorityDto(x.Priority.ToString(), x.Count))
            .OrderBy(x => x.Priority)
            .ToList();

        // ── Stages breakdown by stage type ────────────────────────────────────────
        // For active WOs only — gives "what's currently on the floor"
        var stageGroups = await db.WorkOrderStages.AsNoTracking()
            .Where(s => s.WorkOrder.Status == WorkOrderStatus.Released
                     || s.WorkOrder.Status == WorkOrderStatus.InProgress)
            .GroupBy(s => s.Stage)
            .Select(g => new
            {
                Stage = g.Key,
                InProgress = g.Count(s => s.Status == WorkOrderStageStatus.InProgress),
                Pending = g.Count(s => s.Status == WorkOrderStageStatus.Pending),
                Completed = g.Count(s => s.Status == WorkOrderStageStatus.Completed),
            })
            .ToListAsync(ct);

        // Per-stage loss (across ALL completed stages, not just this month)
        var lossByStage = await db.WorkOrderStages.AsNoTracking()
            .Where(s => s.Status == WorkOrderStageStatus.Completed
                     && s.WeightInGrams.HasValue
                     && s.WeightOutGrams.HasValue)
            .GroupBy(s => s.Stage)
            .Select(g => new
            {
                Stage = g.Key,
                Loss = g.Sum(s => (s.WeightInGrams ?? 0) - (s.WeightOutGrams ?? 0))
            })
            .ToListAsync(ct);

        var lossDict = lossByStage.ToDictionary(x => x.Stage, x => x.Loss);

        var stagesInProgress = stageGroups
            .Select(g => new StagesByStageDto(
                g.Stage.ToString(),
                g.InProgress,
                g.Pending,
                g.Completed,
                lossDict.TryGetValue(g.Stage, out var loss) && loss != 0 ? loss : null))
            .OrderBy(x => x.Stage)
            .ToList();

        // ── Worker workload ───────────────────────────────────────────────────────
        var workerLoad = await db.WorkOrderStages.AsNoTracking()
            .Where(s => s.AssignedWorkerId != null
                     && (s.Status == WorkOrderStageStatus.Pending
                       || s.Status == WorkOrderStageStatus.InProgress))
            .GroupBy(s => new
            {
                s.AssignedWorkerId,
                s.AssignedWorker!.EmployeeCode,
                s.AssignedWorker.FullName,
                s.AssignedWorker.Position
            })
            .Select(g => new
            {
                g.Key.AssignedWorkerId,
                g.Key.EmployeeCode,
                g.Key.FullName,
                Position = g.Key.Position,
                InProgressStages = g.Count(s => s.Status == WorkOrderStageStatus.InProgress),
                PendingStages = g.Count(s => s.Status == WorkOrderStageStatus.Pending),
            })
            .OrderByDescending(g => g.InProgressStages)
            .ThenByDescending(g => g.PendingStages)
            .ToListAsync(ct);

        var workerLoadDto = workerLoad
            .Select(w => new WorkerLoadDto(
                w.AssignedWorkerId!.Value,
                w.EmployeeCode, w.FullName, w.Position.ToString(),
                w.InProgressStages, w.PendingStages))
            .ToList();

        return new ProductionLoadReportDto(
            ActiveWorkOrders: activeCount,
            CompletedThisMonth: completedThisMonth,
            CancelledThisMonth: cancelledThisMonth,
            TotalLossGramsThisMonth: totalLossGrams,
            WorkOrdersByStatus: byStatus,
            WorkOrdersByPriority: byPriority,
            StagesInProgress: stagesInProgress,
            WorkerLoad: workerLoadDto);
    }
}
