using Ficto.Generated.Models;
using Ficto.Services.Content;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Services.Content.Interfaces;

public partial interface IContentService
{
    /// <summary>
    /// Fetches all products, optionally filtered by one or more <c>product_category</c> taxonomy term codenames.
    /// </summary>
    /// <param name="categoryCodenames">
    /// Optional set of taxonomy term codenames. When non-empty, only products tagged with ANY of these terms are returned.
    /// </param>
    /// <returns>A list of Product content items, or an empty list if none found.</returns>
    /// <exception cref="ContentDeliveryException">Thrown when the Delivery API returns a server error (5xx).</exception>
    Task<IReadOnlyList<IContentItem<Product>>> GetProductsAsync(
        IReadOnlyCollection<string>? categoryCodenames = null);

    /// <summary>
    /// Fetches a product by its URL slug.
    /// </summary>
    /// <param name="slug">The URL-friendly slug of the product.</param>
    /// <returns>The Product content item, or null if not found.</returns>
    /// <exception cref="ContentDeliveryException">Thrown when the Delivery API returns a server error (5xx).</exception>
    Task<IContentItem<Product>?> GetProductBySlugAsync(string slug);
}
