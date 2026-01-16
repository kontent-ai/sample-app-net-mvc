namespace Ficto.Models.Helpers;

public record UrlReference : Reference
{
    public override string ReferenceType => "url";
    public string? Url { get; init; }
}