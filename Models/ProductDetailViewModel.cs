namespace Ficto.Models;

public record ProductDetailViewModel
{
    public ProductViewModel Product { get; init; } = new();
    public IReadOnlyList<ProductViewModel> Related { get; init; } = [];
}
