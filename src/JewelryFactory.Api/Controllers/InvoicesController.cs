using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.Shipping.Commands.CreateInvoice;
using JewelryFactory.Application.Features.Shipping.Commands.UpdateInvoiceStatus;
using JewelryFactory.Application.Features.Shipping.DTOs;
using JewelryFactory.Application.Features.Shipping.Queries.GetInvoiceById;
using JewelryFactory.Application.Features.Shipping.Queries.GetInvoicesPaged;
using JewelryFactory.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryFactory.Api.Controllers;

[ApiController]
[Route("api/v1/invoices")]
[Authorize]
public class InvoicesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] InvoiceStatus? status = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var r = await mediator.Send(new GetInvoicesPagedQuery(page, pageSize, status, customerId, search), ct);
        return Ok(ApiResponse<PagedResult<InvoiceSummary>>.Ok(r));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var r = await mediator.Send(new GetInvoiceByIdQuery(id), ct);
        return Ok(ApiResponse<InvoiceResponse>.Ok(r));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Sales")]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceDto dto, CancellationToken ct)
    {
        var id = await mediator.Send(new CreateInvoiceCommand(dto), ct);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Guid>.Ok(id));
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,Manager,Sales")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateInvoiceStatusDto dto, CancellationToken ct)
    {
        await mediator.Send(new UpdateInvoiceStatusCommand(id, dto), ct);
        return NoContent();
    }
}
