using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.SalesOrders.Commands.ChangeSalesOrderStatus;
using JewelryFactory.Application.Features.SalesOrders.Commands.CreateSalesOrder;
using JewelryFactory.Application.Features.SalesOrders.Commands.UpdateSalesOrder;
using JewelryFactory.Application.Features.SalesOrders.DTOs;
using JewelryFactory.Application.Features.SalesOrders.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryFactory.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/sales-orders")]
public class SalesOrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] GetSalesOrdersQuery query, CancellationToken ct)
        => Ok(ApiResponse<PagedResult<SalesOrderResponseDto>>.Ok(await mediator.Send(query, ct)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(ApiResponse<SalesOrderResponseDto>.Ok(await mediator.Send(new GetSalesOrderByIdQuery(id), ct)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSalesOrderDto dto, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateSalesOrderCommand(dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<SalesOrderResponseDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSalesOrderDto dto, CancellationToken ct)
        => Ok(ApiResponse<SalesOrderResponseDto>.Ok(await mediator.Send(new UpdateSalesOrderCommand(id, dto), ct)));

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeSalesOrderStatusDto dto, CancellationToken ct)
        => Ok(ApiResponse<SalesOrderResponseDto>.Ok(await mediator.Send(new ChangeSalesOrderStatusCommand(id, dto), ct)));
}
