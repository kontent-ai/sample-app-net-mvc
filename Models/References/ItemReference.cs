namespace Ficto.Models.References;

public record ItemReference : Reference
{
    public string? Slug { get; init; }
    public string? Type { get; init; }
}