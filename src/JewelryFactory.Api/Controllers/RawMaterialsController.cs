using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.Inventory.RawMaterials.Commands.AdjustRawMaterial;
using JewelryFactory.Application.Features.Inventory.RawMaterials.Commands.ReceiveRawMaterial;
using JewelryFactory.Application.Features.Inventory.RawMaterials.DTOs;
using JewelryFactory.Application.Features.Inventory.RawMaterials.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryFactory.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/inventory/raw-materials")]
public class RawMaterialsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] GetRawMaterialsQuery query, CancellationToken ct)
        => Ok(ApiResponse<PagedResult<RawMaterialResponseDto>>.Ok(await mediator.Send(query, ct)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(ApiResponse<RawMaterialResponseDto>.Ok(await mediator.Send(new GetRawMaterialByIdQuery(id), ct)));

    [HttpPost("receive")]
    public async Task<IActionResult> Receive([FromBody] ReceiveRawMaterialDto dto, CancellationToken ct)
    {
        var result = await mediator.Send(new ReceiveRawMaterialCommand(dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<RawMaterialResponseDto>.Ok(result));
    }

    [HttpPost("{id:guid}/adjust")]
    public async Task<IActionResult> Adjust(Guid id, [FromBody] AdjustRawMaterialDto dto, CancellationToken ct)
        => Ok(ApiResponse<RawMaterialResponseDto>.Ok(await mediator.Send(new AdjustRawMaterialCommand(id, dto), ct)));
}
