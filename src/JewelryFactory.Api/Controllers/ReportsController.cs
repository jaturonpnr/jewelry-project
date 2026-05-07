using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.Reports.DTOs;
using JewelryFactory.Application.Features.Reports.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryFactory.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/reports")]
public class ReportsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Inventory snapshot — totals, stock by status, raw material breakdown.
    /// </summary>
    [HttpGet("inventory-summary")]
    public async Task<IActionResult> InventorySummary(CancellationToken ct)
        => Ok(ApiResponse<InventorySummaryReportDto>.Ok(
            await mediator.Send(new GetInventorySummaryReportQuery(), ct)));

    /// <summary>
    /// Sales order pipeline — by status, top customers, revenue by currency, overdue.
    /// </summary>
    [HttpGet("sales-order-pipeline")]
    public async Task<IActionResult> SalesOrderPipeline(CancellationToken ct)
        => Ok(ApiResponse<SalesOrderPipelineReportDto>.Ok(
            await mediator.Send(new GetSalesOrderPipelineReportQuery(), ct)));

    /// <summary>
    /// Production load — active WOs, stages in progress, worker workload, monthly loss.
    /// </summary>
    [HttpGet("production-load")]
    public async Task<IActionResult> ProductionLoad(CancellationToken ct)
        => Ok(ApiResponse<ProductionLoadReportDto>.Ok(
            await mediator.Send(new GetProductionLoadReportQuery(), ct)));
}
