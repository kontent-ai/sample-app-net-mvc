namespace Ficto.Models;

public record CategoryTermViewModel
{
    public string Codename { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsSelected { get; init; }
    public IReadOnlyList<CategoryTermViewModel> Children { get; init; } = [];
}
