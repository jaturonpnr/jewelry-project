using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.Inventory.StoneParcels.Commands;
using JewelryFactory.Application.Features.Inventory.StoneParcels.DTOs;
using JewelryFactory.Application.Features.Inventory.StoneParcels.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryFactory.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/inventory/stone-parcels")]
public class StoneParcelsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] GetStoneParcelsQuery query, CancellationToken ct)
        => Ok(ApiResponse<PagedResult<StoneParcelResponseDto>>.Ok(await mediator.Send(query, ct)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(ApiResponse<StoneParcelResponseDto>.Ok(await mediator.Send(new GetStoneParcelByIdQuery(id), ct)));

    [HttpPost("receive")]
    public async Task<IActionResult> Receive([FromBody] ReceiveStoneParcelDto dto, CancellationToken ct)
    {
        var result = await mediator.Send(new ReceiveStoneParcelCommand(dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<StoneParcelResponseDto>.Ok(result));
    }

    [HttpPost("{id:guid}/adjust")]
    public async Task<IActionResult> Adjust(Guid id, [FromBody] AdjustStoneParcelDto dto, CancellationToken ct)
        => Ok(ApiResponse<StoneParcelResponseDto>.Ok(await mediator.Send(new AdjustStoneParcelCommand(id, dto), ct)));
}
