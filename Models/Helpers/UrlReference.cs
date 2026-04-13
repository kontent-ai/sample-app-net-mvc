namespace Ficto.Models.Helpers;

public record UrlReference : Reference
{
    public string? Url { get; init; }
}