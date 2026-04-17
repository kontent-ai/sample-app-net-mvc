using Ficto.Generated.Models;
using Ficto.Models;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Services.Content.Interfaces;

public partial interface IContentService
{
    /// <summary>
    /// Fetches a page of articles ordered by publishing date (newest first).
    /// Demonstrates server-side pagination via Delivery API <c>skip</c>/<c>limit</c> +
    /// <c>includeTotalCount</c>.
    /// </summary>
    /// <param name="skip">Number of items to skip (zero-based offset).</param>
    /// <param name="limit">Maximum number of items to return.</param>
    /// <returns>A <see cref="PagedResult{T}"/> of Article content items.</returns>
    /// <exception cref="ContentDeliveryException">Thrown when the Delivery API returns a server error (5xx).</exception>
    Task<PagedResult<IContentItem<Article>>> GetArticlesAsync(int skip = 0, int limit = 12);

    /// <summary>
    /// Fetches an article by its URL slug.
    /// </summary>
    /// <param name="slug">The URL-friendly slug of the article.</param>
    /// <returns>The Article content item, or null if not found.</returns>
    /// <exception cref="ContentDeliveryException">Thrown when the Delivery API returns a server error (5xx).</exception>
    Task<IContentItem<Article>?> GetArticleBySlugAsync(string slug);
}
