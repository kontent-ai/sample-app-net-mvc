namespace Ficto.Models;

/// <summary>
/// Paged response wrapping the items returned for a query plus pagination metadata.
/// <see cref="TotalCount"/> comes from the Delivery API when <c>WithTotalCount()</c> is applied,
/// enabling UIs to show "Showing 1-12 of 47" style labels.
/// </summary>
public record PagedResult<T>(IReadOnlyList<T> Items, int? TotalCount, int Skip, int Limit)
{
    public static PagedResult<T> Empty(int skip, int limit) => new([], 0, skip, limit);

    public int PageNumber => Limit > 0 ? (Skip / Limit) + 1 : 1;
    public int PageCount => TotalCount is { } total && Limit > 0
        ? (int)Math.Ceiling(total / (double)Limit)
        : 1;
    public bool HasPrevious => Skip > 0;
    public bool HasNext => TotalCount is { } total ? Skip + Items.Count < total : Items.Count == Limit;
}
