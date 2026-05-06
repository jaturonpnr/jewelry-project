using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.Workers.Commands;
using JewelryFactory.Application.Features.Workers.DTOs;
using JewelryFactory.Application.Features.Workers.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryFactory.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/workers")]
public class WorkersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] GetWorkersQuery query, CancellationToken ct)
        => Ok(ApiResponse<PagedResult<WorkerResponseDto>>.Ok(await mediator.Send(query, ct)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(ApiResponse<WorkerResponseDto>.Ok(await mediator.Send(new GetWorkerByIdQuery(id), ct)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkerDto dto, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateWorkerCommand(dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<WorkerResponseDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWorkerDto dto, CancellationToken ct)
        => Ok(ApiResponse<WorkerResponseDto>.Ok(await mediator.Send(new UpdateWorkerCommand(id, dto), ct)));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteWorkerCommand(id), ct);
        return NoContent();
    }
}
