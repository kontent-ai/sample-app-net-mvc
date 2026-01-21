using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models;

public record SolutionViewModel
{
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public IAsset? MainImage { get; init; }
    public string? Showcase { get; init; }
    public IReadOnlyList<string> ImagingTechnology { get; init; } = [];
    public string? MetadataTitle { get; init; }
    public string? MetadataDescription { get; init; }
    public string? MetadataKeywords { get; init; }
}
