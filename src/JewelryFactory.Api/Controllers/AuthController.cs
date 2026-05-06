using JewelryFactory.Application.Common.Models;
using JewelryFactory.Application.Features.Auth.Commands.Login;
using JewelryFactory.Application.Features.Auth.Commands.RefreshToken;
using JewelryFactory.Application.Features.Auth.Commands.Register;
using JewelryFactory.Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryFactory.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    /// <summary>POST /api/v1/auth/register</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request, CancellationToken ct)
    {
        var result = await mediator.Send(new RegisterCommand(request), ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<AuthResponseDto>.Ok(result));
    }

    /// <summary>POST /api/v1/auth/login</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken ct)
    {
        var result = await mediator.Send(new LoginCommand(request), ct);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result));
    }

    /// <summary>POST /api/v1/auth/refresh</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request, CancellationToken ct)
    {
        var result = await mediator.Send(new RefreshTokenCommand(request), ct);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result));
    }

    /// <summary>GET /api/v1/auth/me — sanity check that JWT is valid</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Me()
    {
        // Group claims (sub/nameidentifier collapse onto the same key after JWT default mapping).
        var claims = User.Claims
            .GroupBy(c => c.Type)
            .Select(g => new { type = g.Key, values = g.Select(c => c.Value).ToArray() });
        return Ok(ApiResponse<object>.Ok(claims));
    }
}
