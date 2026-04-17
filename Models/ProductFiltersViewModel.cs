namespace Ficto.Models;

public record ProductFiltersViewModel
{
    public IReadOnlyList<CategoryTermViewModel> Categories { get; init; } = [];
    public bool HasSelection { get; init; }
}
