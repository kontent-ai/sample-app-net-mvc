using Ficto.Generated.Models;

namespace Ficto.Models.Mappers;

public class ProductMapper : IMapper<Product, ProductViewModel>
{
    public ProductViewModel Map(Product source)
    {
        return new ProductViewModel
        {
            Slug = source.Slug,
            Name = source.ProductBaseName,
            Description = source.ProductBaseDescription,
            MainImage = source.ProductBaseMainImage.FirstOrDefault(),
            Price = source.Price,
            Category = source.Category.Select(t => t.Codename).ToList(),
            MetadataTitle = source.MetadataTitle,
            MetadataDescription = source.MetadataDescription,
            MetadataKeywords = source.MetadataKeywords
        };
    }
}
