using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.Shipping.Commands.CreateShipment;
using JewelryFactory.Application.Features.Shipping.Commands.UpdateShipmentStatus;
using JewelryFactory.Application.Features.Shipping.DTOs;
using JewelryFactory.Application.Features.Shipping.Queries.GetShipmentById;
using JewelryFactory.Application.Features.Shipping.Queries.GetShipmentsPaged;
using JewelryFactory.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryFactory.Api.Controllers;

[ApiController]
[Route("api/v1/shipments")]
[Authorize]
public class ShipmentsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] ShipmentStatus? status = null, [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var r = await mediator.Send(new GetShipmentsPagedQuery(page, pageSize, status, search), ct);
        return Ok(ApiResponse<PagedResult<ShipmentSummary>>.Ok(r));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var r = await mediator.Send(new GetShipmentByIdQuery(id), ct);
        return Ok(ApiResponse<ShipmentResponse>.Ok(r));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Sales")]
    public async Task<IActionResult> Create([FromBody] CreateShipmentDto dto, CancellationToken ct)
    {
        var id = await mediator.Send(new CreateShipmentCommand(dto), ct);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Guid>.Ok(id));
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,Manager,Sales")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateShipmentStatusDto dto, CancellationToken ct)
    {
        await mediator.Send(new UpdateShipmentStatusCommand(id, dto), ct);
        return NoContent();
    }
}
