namespace Ficto.Models;

public record ProductViewModel
{
    public Guid? ItemId { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public AssetViewModel? MainImage { get; init; }
    public double? Price { get; init; }
    public string? Category { get; init; }
    public string? MetadataTitle { get; init; }
    public string? MetadataDescription { get; init; }
    public string? MetadataKeywords { get; init; }
}
