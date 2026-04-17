using Ficto.Generated.Models;
using Ficto.Models;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Services.Content;

public partial interface IContentService
{
    /// <summary>
    /// Fetches a page of products, optionally filtered by one or more <c>product_category</c> taxonomy term codenames.
    /// Demonstrates server-side pagination via Delivery API <c>skip</c>/<c>limit</c> +
    /// <c>includeTotalCount</c>.
    /// </summary>
    /// <param name="categoryCodenames">
    /// Optional set of taxonomy term codenames. When non-empty, only products tagged with ANY of these terms are returned.
    /// </param>
    /// <param name="skip">Number of items to skip (zero-based offset).</param>
    /// <param name="limit">Maximum number of items to return.</param>
    /// <returns>A <see cref="PagedResult{T}"/> of Product content items.</returns>
    /// <exception cref="ContentDeliveryException">Thrown when the Delivery API returns a server error (5xx).</exception>
    Task<PagedResult<IContentItem<Product>>> GetProductsAsync(
        IReadOnlyCollection<string>? categoryCodenames = null,
        int skip = 0,
        int limit = 12);

    /// <summary>
    /// Fetches a product by its URL slug.
    /// </summary>
    /// <param name="slug">The URL-friendly slug of the product.</param>
    /// <returns>The Product content item, or null if not found.</returns>
    /// <exception cref="ContentDeliveryException">Thrown when the Delivery API returns a server error (5xx).</exception>
    Task<IContentItem<Product>?> GetProductBySlugAsync(string slug);

    /// <summary>
    /// Fetches all products in a category (no paging). Used for small in-memory lookups
    /// like the "related products" strip on a product detail page.
    /// </summary>
    Task<IReadOnlyList<IContentItem<Product>>> GetProductsByCategoryAsync(
        IReadOnlyCollection<string> categoryCodenames,
        int limit = 4);
}
