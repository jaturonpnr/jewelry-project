using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.Bom.Commands.CreateBomTemplate;
using JewelryFactory.Application.Features.Bom.Commands.DeleteBomTemplate;
using JewelryFactory.Application.Features.Bom.Commands.UpdateBomTemplate;
using JewelryFactory.Application.Features.Bom.DTOs;
using JewelryFactory.Application.Features.Bom.Queries.GetBomTemplateById;
using JewelryFactory.Application.Features.Bom.Queries.GetBomTemplatesPaged;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryFactory.Api.Controllers;

[ApiController]
[Route("api/v1/bom-templates")]
[Authorize]
public class BomTemplatesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetBomTemplatesPagedQuery(page, pageSize, search, isActive), ct);
        return Ok(ApiResponse<PagedResult<BomTemplateSummaryResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetBomTemplateByIdQuery(id), ct);
        return Ok(ApiResponse<BomTemplateResponse>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Designer")]
    public async Task<IActionResult> Create([FromBody] CreateBomTemplateDto dto, CancellationToken ct)
    {
        var id = await mediator.Send(new CreateBomTemplateCommand(dto), ct);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Guid>.Ok(id));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Designer")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBomTemplateDto dto, CancellationToken ct)
    {
        await mediator.Send(new UpdateBomTemplateCommand(id, dto), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteBomTemplateCommand(id), ct);
        return NoContent();
    }
}
