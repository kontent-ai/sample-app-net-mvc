namespace Ficto.Models.Helpers;

public record ItemReference : Reference
{
    public string? Slug { get; init; }
    public string? Type { get; init; }
}