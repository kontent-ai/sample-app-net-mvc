using Ficto.Generated.Models;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Services.Content;

public partial interface IContentService
{
    /// <summary>
    /// Fetches the homepage (website root) content item.
    /// </summary>
    /// <returns>The WebsiteRoot content item, or null if not found.</returns>
    /// <exception cref="ContentDeliveryException">Thrown when the Delivery API returns a server error (5xx).</exception>
    Task<IContentItem<WebsiteRoot>?> GetHomepageAsync(CancellationToken ct = default);

    /// <summary>
    /// Fetches a page by its URL slug.
    /// </summary>
    /// <param name="slug">The URL-friendly slug of the page.</param>
    /// <param name="ct">Cancellation token forwarded to the Delivery API call.</param>
    /// <returns>The Page content item, or null if not found.</returns>
    /// <exception cref="ContentDeliveryException">Thrown when the Delivery API returns a server error (5xx).</exception>
    Task<IContentItem<Page>?> GetPageBySlugAsync(string slug, CancellationToken ct = default);
}
