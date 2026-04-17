using Ficto.Generated.Models;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Mappers;

public class ProductMapper : IAsyncMapper<IContentItem<Product>, ProductViewModel>
{
    public Task<ProductViewModel> MapAsync(IContentItem<Product> source)
    {
        var e = source.Elements;
        return Task.FromResult(new ProductViewModel
        {
            ItemId = source.System.Id,
            Slug = e.Slug,
            Name = e.ProductBaseName,
            Description = e.ProductBaseDescription,
            MainImage = AssetViewModel.From(e.ProductBaseMainImage?.FirstOrDefault()),
            Price = e.Price,
            Category = e.Category?.FirstOrDefault()?.Codename,
            MetadataTitle = e.MetadataTitle,
            MetadataDescription = e.MetadataDescription,
            MetadataKeywords = e.MetadataKeywords
        });
    }
}
