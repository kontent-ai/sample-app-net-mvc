using Ficto.Models.Helpers;

namespace Ficto.Models;

public record WebsiteRootViewModel
{
    public string Title { get; init; } = string.Empty;
    public IReadOnlyList<PageBlockViewModel> Content { get; init; } = [];
    public string? MetadataTitle { get; init; }
    public string? MetadataDescription { get; init; }
    public string? MetadataKeywords { get; init; }
}
