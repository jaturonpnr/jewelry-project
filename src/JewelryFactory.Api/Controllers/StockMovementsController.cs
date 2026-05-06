using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.Inventory.StockMovements.DTOs;
using JewelryFactory.Application.Features.Inventory.StockMovements.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryFactory.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/inventory/movements")]
public class StockMovementsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] GetStockMovementsQuery query, CancellationToken ct)
        => Ok(ApiResponse<PagedResult<StockMovementResponseDto>>.Ok(await mediator.Send(query, ct)));
}
