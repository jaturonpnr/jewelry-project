using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.MaterialTypes.Commands;
using JewelryFactory.Application.Features.MaterialTypes.DTOs;
using JewelryFactory.Application.Features.MaterialTypes.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryFactory.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/material-types")]
public class MaterialTypesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] GetMaterialTypesQuery query, CancellationToken ct)
        => Ok(ApiResponse<PagedResult<MaterialTypeResponseDto>>.Ok(await mediator.Send(query, ct)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(ApiResponse<MaterialTypeResponseDto>.Ok(await mediator.Send(new GetMaterialTypeByIdQuery(id), ct)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMaterialTypeDto dto, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateMaterialTypeCommand(dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<MaterialTypeResponseDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMaterialTypeDto dto, CancellationToken ct)
        => Ok(ApiResponse<MaterialTypeResponseDto>.Ok(await mediator.Send(new UpdateMaterialTypeCommand(id, dto), ct)));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteMaterialTypeCommand(id), ct);
        return NoContent();
    }
}
