namespace Ficto.Models;

public record ArticleListingViewModel
{
    public IReadOnlyList<PageBlockViewModel> HeaderContent { get; init; } = [];
    public IReadOnlyList<ArticleViewModel> Articles { get; init; } = [];
    public PagerViewModel Pager { get; init; } = PagerViewModel.Empty;
}
