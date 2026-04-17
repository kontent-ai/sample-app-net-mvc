namespace Ficto.Models;

/// <summary>
/// View-facing projection of <see cref="PagedResult{T}"/>, decoupled from the item type so a single
/// <c>_Pager.cshtml</c> partial renders paging for any listing.
/// </summary>
public record PagerViewModel
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; }
    public int ItemsOnPage { get; init; }
    public int? TotalCount { get; init; }
    public bool HasPrevious { get; init; }
    public bool HasNext { get; init; }
    public int PageCount { get; init; } = 1;

    /// <summary>
    /// Non-paging query string values to preserve across page links (e.g., active filters).
    /// Values are pre-formatted exactly as the app expects to re-bind them (multi-valued filters
    /// that must round-trip as repeated query keys should be joined by the caller's convention).
    /// </summary>
    public IReadOnlyList<KeyValuePair<string, string>> ExtraQuery { get; init; } = [];

    public static PagerViewModel Empty => new();

    public static PagerViewModel From<T>(
        PagedResult<T> result,
        IReadOnlyList<KeyValuePair<string, string>>? extraQuery = null) => new()
        {
            PageNumber = result.PageNumber,
            PageSize = result.Limit,
            ItemsOnPage = result.Items.Count,
            TotalCount = result.TotalCount,
            HasPrevious = result.HasPrevious,
            HasNext = result.HasNext,
            PageCount = result.PageCount,
            ExtraQuery = extraQuery ?? [],
        };

    public int FirstItemNumber => ItemsOnPage == 0 ? 0 : ((PageNumber - 1) * PageSize) + 1;
    public int LastItemNumber => FirstItemNumber + ItemsOnPage - 1;
}
