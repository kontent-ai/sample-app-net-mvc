namespace Ficto.Models.Helpers;

public abstract record Reference
{
    public string? Label { get; init; }
    public string? Caption { get; init; }
}