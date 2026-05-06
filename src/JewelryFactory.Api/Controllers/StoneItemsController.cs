using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.Inventory.StoneItems.Commands;
using JewelryFactory.Application.Features.Inventory.StoneItems.DTOs;
using JewelryFactory.Application.Features.Inventory.StoneItems.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryFactory.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/inventory/stone-items")]
public class StoneItemsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] GetStoneItemsQuery query, CancellationToken ct)
        => Ok(ApiResponse<PagedResult<StoneItemResponseDto>>.Ok(await mediator.Send(query, ct)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(ApiResponse<StoneItemResponseDto>.Ok(await mediator.Send(new GetStoneItemByIdQuery(id), ct)));

    [HttpPost("receive")]
    public async Task<IActionResult> Receive([FromBody] ReceiveStoneItemDto dto, CancellationToken ct)
    {
        var result = await mediator.Send(new ReceiveStoneItemCommand(dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<StoneItemResponseDto>.Ok(result));
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStoneItemStatusDto dto, CancellationToken ct)
        => Ok(ApiResponse<StoneItemResponseDto>.Ok(await mediator.Send(new UpdateStoneItemStatusCommand(id, dto), ct)));
}
