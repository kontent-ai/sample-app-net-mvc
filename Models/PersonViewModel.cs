namespace Ficto.Models;

public record PersonViewModel
{
    public string Name { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Occupation { get; init; }
    public AssetViewModel? Photograph { get; init; }
    public string? Bio { get; init; }
}