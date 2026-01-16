using Ficto.Models.Helpers;

namespace Ficto.Models;

public record PageViewModel
{
    public string? Title { get; init; }
    public string Slug { get; init; } = string.Empty;
    public IReadOnlyList<PageBlockViewModel> Content { get; init; } = [];
}