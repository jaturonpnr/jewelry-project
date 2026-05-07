using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.WorkOrders.Commands;
using JewelryFactory.Application.Features.WorkOrders.Commands.ChangeWorkOrderStatus;
using JewelryFactory.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using JewelryFactory.Application.Features.WorkOrders.Commands.UpdateWorkOrder;
using JewelryFactory.Application.Features.WorkOrders.DTOs;
using JewelryFactory.Application.Features.WorkOrders.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryFactory.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/work-orders")]
public class WorkOrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] GetWorkOrdersQuery query, CancellationToken ct)
        => Ok(ApiResponse<PagedResult<WorkOrderResponseDto>>.Ok(await mediator.Send(query, ct)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(ApiResponse<WorkOrderResponseDto>.Ok(await mediator.Send(new GetWorkOrderByIdQuery(id), ct)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkOrderDto dto, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateWorkOrderCommand(dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<WorkOrderResponseDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWorkOrderDto dto, CancellationToken ct)
        => Ok(ApiResponse<WorkOrderResponseDto>.Ok(await mediator.Send(new UpdateWorkOrderCommand(id, dto), ct)));

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeWorkOrderStatusDto dto, CancellationToken ct)
        => Ok(ApiResponse<WorkOrderResponseDto>.Ok(await mediator.Send(new ChangeWorkOrderStatusCommand(id, dto), ct)));

    // ── Stage operations ────────────────────────────────────────────────

    [HttpPost("{id:guid}/stages/{stageId:guid}/start")]
    public async Task<IActionResult> StartStage(Guid id, Guid stageId, [FromBody] StartStageDto dto, CancellationToken ct)
        => Ok(ApiResponse<WorkOrderStageResponseDto>.Ok(await mediator.Send(new StartStageCommand(id, stageId, dto), ct)));

    [HttpPost("{id:guid}/stages/{stageId:guid}/complete")]
    public async Task<IActionResult> CompleteStage(Guid id, Guid stageId, [FromBody] CompleteStageDto dto, CancellationToken ct)
        => Ok(ApiResponse<WorkOrderStageResponseDto>.Ok(await mediator.Send(new CompleteStageCommand(id, stageId, dto), ct)));

    [HttpPost("{id:guid}/stages/{stageId:guid}/skip")]
    public async Task<IActionResult> SkipStage(Guid id, Guid stageId, [FromBody] SkipStageDto dto, CancellationToken ct)
        => Ok(ApiResponse<WorkOrderStageResponseDto>.Ok(await mediator.Send(new SkipStageCommand(id, stageId, dto), ct)));

    [HttpPost("{id:guid}/stages/{stageId:guid}/fail")]
    public async Task<IActionResult> FailStage(Guid id, Guid stageId, [FromBody] FailStageDto dto, CancellationToken ct)
        => Ok(ApiResponse<WorkOrderStageResponseDto>.Ok(await mediator.Send(new FailStageCommand(id, stageId, dto), ct)));
}
