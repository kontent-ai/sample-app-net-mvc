namespace Ficto.Models;

public record NavigationViewModel
{
    public Guid? ItemId { get; init; }
    public string? Label { get; init; }
    public string Url { get; init; } = "#";
    public bool IsExternal { get; init; }
    public IReadOnlyList<NavigationViewModel> Subitems { get; init; } = [];
}