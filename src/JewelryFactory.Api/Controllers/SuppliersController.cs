using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.Suppliers.Commands.CreateSupplier;
using JewelryFactory.Application.Features.Suppliers.Commands.DeleteSupplier;
using JewelryFactory.Application.Features.Suppliers.Commands.UpdateSupplier;
using JewelryFactory.Application.Features.Suppliers.DTOs;
using JewelryFactory.Application.Features.Suppliers.Queries.GetSupplierById;
using JewelryFactory.Application.Features.Suppliers.Queries.GetSuppliers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryFactory.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/suppliers")]
public class SuppliersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] GetSuppliersQuery query, CancellationToken ct)
        => Ok(ApiResponse<PagedResult<SupplierResponseDto>>.Ok(await mediator.Send(query, ct)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(ApiResponse<SupplierResponseDto>.Ok(await mediator.Send(new GetSupplierByIdQuery(id), ct)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupplierDto dto, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateSupplierCommand(dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<SupplierResponseDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSupplierDto dto, CancellationToken ct)
        => Ok(ApiResponse<SupplierResponseDto>.Ok(await mediator.Send(new UpdateSupplierCommand(id, dto), ct)));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteSupplierCommand(id), ct);
        return NoContent();
    }
}
