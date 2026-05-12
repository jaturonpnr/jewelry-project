using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.QualityControl.Commands.CreateQcInspection;
using JewelryFactory.Application.Features.QualityControl.Commands.DeleteQcInspection;
using JewelryFactory.Application.Features.QualityControl.DTOs;
using JewelryFactory.Application.Features.QualityControl.Queries.GetQcInspectionById;
using JewelryFactory.Application.Features.QualityControl.Queries.GetQcInspectionsPaged;
using JewelryFactory.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryFactory.Api.Controllers;

[ApiController]
[Route("api/v1/qc-inspections")]
[Authorize]
public class QcInspectionsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] QcInspectionType? inspectionType = null,
        [FromQuery] QcResult? result = null,
        [FromQuery] Guid? workOrderId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var r = await mediator.Send(
            new GetQcInspectionsPagedQuery(page, pageSize, inspectionType, result, workOrderId, fromDate, toDate), ct);
        return Ok(ApiResponse<PagedResult<QcInspectionSummary>>.Ok(r));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var r = await mediator.Send(new GetQcInspectionByIdQuery(id), ct);
        return Ok(ApiResponse<QcInspectionResponse>.Ok(r));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Qc,Production")]
    public async Task<IActionResult> Create([FromBody] CreateQcInspectionDto dto, CancellationToken ct)
    {
        var id = await mediator.Send(new CreateQcInspectionCommand(dto), ct);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Guid>.Ok(id));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteQcInspectionCommand(id), ct);
        return NoContent();
    }
}
