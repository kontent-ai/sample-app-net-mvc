using Ficto.Generated.Models;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Services.Content.Interfaces;

public partial interface IContentService
{
    /// <summary>
    /// Fetches all products.
    /// </summary>
    /// <returns>A list of Product content items.</returns>
    Task<IReadOnlyList<IContentItem<Product>>> GetProductsAsync();

    /// <summary>
    /// Fetches a product by its URL slug.
    /// </summary>
    /// <param name="slug">The URL-friendly slug of the product.</param>
    /// <returns>The Product content item, or null if not found.</returns>
    Task<IContentItem<Product>?> GetProductBySlugAsync(string slug);
}
