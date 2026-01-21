using Ficto.Models.Helpers;

namespace Ficto.Models;

public record WebsiteRootViewModel
{
    public string Title { get; init; } = string.Empty;
    public IReadOnlyList<NavigationViewModel> Navigation { get; init; } = [];
    public IReadOnlyList<PageBlockViewModel> Content { get; init; } = [];
    public IReadOnlyList<PageViewModel> Subpages { get; init; } = [];
    public string? MetadataTitle { get; init; }
    public string? MetadataDescription { get; init; }
    public string? MetadataKeywords { get; init; }
}
