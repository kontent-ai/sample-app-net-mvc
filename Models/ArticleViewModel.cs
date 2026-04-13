namespace Ficto.Models;

public record ArticleViewModel
{
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string ArticleType { get; init; } = string.Empty;
    public string? Abstract { get; init; }
    public string? Content { get; init; }
    public AssetViewModel? HeroImage { get; init; }
    public PersonViewModel? Author { get; init; }
    public string? MetadataDescription { get; init; }
    public string? MetadataKeywords { get; init; }
    public string? MetadataTitle { get; init; }
    public DateTime? PublishingDate { get; init; }
}