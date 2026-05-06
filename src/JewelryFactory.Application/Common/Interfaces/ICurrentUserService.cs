namespace JewelryFactory.Application.Common.Interfaces;

/// <summary>
/// Provides access to the currently authenticated user (from HttpContext).
/// Used by audit-trail logic.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? Role { get; }
}
