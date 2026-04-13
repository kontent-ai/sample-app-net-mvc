using Ficto.Models.Helpers;

namespace Ficto.Models;

public record SolutionListingViewModel
{
    public IReadOnlyList<PageBlockViewModel> HeaderContent { get; init; } = [];
    public IReadOnlyList<SolutionViewModel> Solutions { get; init; } = [];
}
