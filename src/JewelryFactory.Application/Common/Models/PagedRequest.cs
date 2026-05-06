namespace JewelryFactory.Application.Common.Models;

/// <summary>
/// Standard pagination + sorting input. Per CLAUDE.md §8.
/// </summary>
public class PagedRequest
{
    private const int MaxPageSize = 100;
    private int _pageSize = 20;

    public int Page { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? 1 : value;
    }

    public string? Sort { get; set; }
    public string? Order { get; set; }   // "asc" | "desc"
    public string? Search { get; set; }
}
