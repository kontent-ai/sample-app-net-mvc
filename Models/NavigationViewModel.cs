using Ficto.Models.Helpers;

namespace Ficto.Models;

public record NavigationViewModel
{
    public Reference? Reference { get; init; }
    public string? Label { get; init; }
    public string? Caption { get; init; }
    public string? ExternalUri { get; init; }
    public string? ContentItemLink { get; init; }
    public IReadOnlyList<NavigationViewModel>? Subitems { get; init; }
}