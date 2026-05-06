using FluentValidation;
using JewelryFactory.Application.Common.Models;
using JewelryFactory.Domain.Exceptions;

namespace JewelryFactory.Api.Middleware;

/// <summary>
/// Global exception → standardized ApiResponse error envelope (per CLAUDE.md §6, §8).
/// </summary>
public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            logger.LogWarning("Validation failed: {Errors}",
                string.Join("; ", ex.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")));

            await WriteAsync(context, StatusCodes.Status400BadRequest,
                ex.Errors.Select(e => new ApiError(e.PropertyName, e.ErrorMessage)).ToArray());
        }
        catch (NotFoundException ex)
        {
            logger.LogWarning("Resource not found: {Message}", ex.Message);
            await WriteAsync(context, StatusCodes.Status404NotFound,
                new[] { new ApiError("resource", ex.Message) });
        }
        catch (UnauthorizedException ex)
        {
            logger.LogWarning("Unauthorized: {Message}", ex.Message);
            await WriteAsync(context, StatusCodes.Status401Unauthorized,
                new[] { new ApiError("auth", ex.Message) });
        }
        catch (BusinessRuleException ex)
        {
            logger.LogWarning("Business rule violation: {Message}", ex.Message);
            await WriteAsync(context, StatusCodes.Status409Conflict,
                new[] { new ApiError("business", ex.Message) });
        }
        catch (ConcurrencyException ex)
        {
            logger.LogWarning("Concurrency conflict: {Message}", ex.Message);
            await WriteAsync(context, StatusCodes.Status409Conflict,
                new[] { new ApiError("concurrency", ex.Message) });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await WriteAsync(context, StatusCodes.Status500InternalServerError,
                new[] { new ApiError("server", "An unexpected error occurred.") });
        }
    }

    private static Task WriteAsync(HttpContext ctx, int statusCode, ApiError[] errors)
    {
        ctx.Response.StatusCode = statusCode;
        ctx.Response.ContentType = "application/json";
        return ctx.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(errors));
    }
}
