using Ficto.Models.Helpers;

namespace Ficto.Models;

public record VisualContainerViewModel : PageBlockViewModel
{
    public string? Title { get; init; }
    public string? Subtitle { get; init; }
    public IReadOnlyList<FactViewModel> Items { get; init; } = [];
    public VisualRepresentation VisualRepresentation { get; init; }
    public override string PartialViewName => "_VisualContainer";
}