namespace Ficto.Models.Helpers;

public abstract record Reference
{
    public abstract string ReferenceType { get; }
    public string? Label { get; init; }
    public string? Caption { get; init; }
}