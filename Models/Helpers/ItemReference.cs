namespace Ficto.Models.Helpers;

public record ItemReference : Reference
{
    public override string ReferenceType => "item";
    public string? Slug { get; init; }
    public string? Type { get; init; }
}