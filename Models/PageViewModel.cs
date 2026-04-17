namespace Ficto.Models;

public record PageViewModel
{
    public Guid? ItemId { get; init; }
    public string? Title { get; init; }
    public string Slug { get; init; } = string.Empty;
    public IReadOnlyList<PageBlockViewModel> Content { get; init; } = [];
    public IReadOnlyList<PageViewModel> Subpages { get; init; } = [];
    public string? MetadataTitle { get; init; }
    public string? MetadataDescription { get; init; }
    public string? MetadataKeywords { get; init; }
}
