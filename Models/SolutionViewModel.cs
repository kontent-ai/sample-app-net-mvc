namespace Ficto.Models;

public record SolutionViewModel
{
    public Guid? ItemId { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public AssetViewModel? MainImage { get; init; }
    public string? Showcase { get; init; }
    public string? ImagingTechnology { get; init; }
    public string? MetadataTitle { get; init; }
    public string? MetadataDescription { get; init; }
    public string? MetadataKeywords { get; init; }
}
