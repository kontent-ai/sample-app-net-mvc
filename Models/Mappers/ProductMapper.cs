using Ficto.Generated.Models;

namespace Ficto.Models.Mappers;

public class ProductMapper : IAsyncMapper<Product, ProductViewModel>
{
    public Task<ProductViewModel> MapAsync(Product source) => Task.FromResult(new ProductViewModel
    {
        Slug = source.Slug,
        Name = source.ProductBaseName,
        Description = source.ProductBaseDescription,
        MainImage = AssetViewModel.From(source.ProductBaseMainImage?.FirstOrDefault()),
        Price = source.Price,
        Category = source.Category?.FirstOrDefault()?.Codename,
        MetadataTitle = source.MetadataTitle,
        MetadataDescription = source.MetadataDescription,
        MetadataKeywords = source.MetadataKeywords
    });
}
