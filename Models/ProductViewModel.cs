using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models;

public record ProductViewModel
{
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public IAsset? MainImage { get; init; }
    public double? Price { get; init; }
    public IReadOnlyList<string> Category { get; init; } = [];
    public string? MetadataTitle { get; init; }
    public string? MetadataDescription { get; init; }
    public string? MetadataKeywords { get; init; }
}
