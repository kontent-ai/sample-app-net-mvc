namespace Ficto.Models;

public record ProductListingViewModel
{
    public IReadOnlyList<PageBlockViewModel> HeaderContent { get; init; } = [];
    public IReadOnlyList<ProductViewModel> Products { get; init; } = [];
    public IReadOnlyList<CategoryTermViewModel> Categories { get; init; } = [];
    public IReadOnlyList<string> SelectedCategories { get; init; } = [];
    public PagerViewModel Pager { get; init; } = PagerViewModel.Empty;
}
