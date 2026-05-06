using System.Security.Claims;
using JewelryFactory.Application.Common.Interfaces;

namespace JewelryFactory.Api.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
            var raw = httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }

    public string? Email =>
        httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);

    public string? Role =>
        httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
}
