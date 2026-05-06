using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.Customers.Commands.CreateCustomer;
using JewelryFactory.Application.Features.Customers.Commands.DeleteCustomer;
using JewelryFactory.Application.Features.Customers.Commands.UpdateCustomer;
using JewelryFactory.Application.Features.Customers.DTOs;
using JewelryFactory.Application.Features.Customers.Queries.GetCustomerById;
using JewelryFactory.Application.Features.Customers.Queries.GetCustomers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryFactory.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/customers")]
public class CustomersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] GetCustomersQuery query, CancellationToken ct)
    {
        var result = await mediator.Send(query, ct);
        return Ok(ApiResponse<PagedResult<CustomerResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCustomerByIdQuery(id), ct);
        return Ok(ApiResponse<CustomerResponseDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateCustomerCommand(dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<CustomerResponseDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerDto dto, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateCustomerCommand(id, dto), ct);
        return Ok(ApiResponse<CustomerResponseDto>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteCustomerCommand(id), ct);
        return NoContent();
    }
}
