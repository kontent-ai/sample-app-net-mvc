namespace Ficto.Models.References;

public abstract record Reference
{
    public string? Label { get; init; }
    public string? Caption { get; init; }
}