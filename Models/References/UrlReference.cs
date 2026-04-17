namespace Ficto.Models.References;

public record UrlReference : Reference
{
    public string? Url { get; init; }
}