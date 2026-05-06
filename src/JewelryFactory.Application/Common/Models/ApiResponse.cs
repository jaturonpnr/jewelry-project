namespace JewelryFactory.Application.Common.Models;

/// <summary>
/// Standard response envelope per CLAUDE.md §8.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public IReadOnlyCollection<ApiError>? Errors { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data) =>
        new() { Success = true, Data = data };

    public static ApiResponse<T> Fail(IReadOnlyCollection<ApiError> errors) =>
        new() { Success = false, Errors = errors };

    public static ApiResponse<T> Fail(string field, string message) =>
        new() { Success = false, Errors = new[] { new ApiError(field, message) } };
}

public record ApiError(string Field, string Message);
