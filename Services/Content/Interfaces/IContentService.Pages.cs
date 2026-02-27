using Ficto.Generated.Models;
using Ficto.Services.Content;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Services.Content.Interfaces;

public partial interface IContentService
{
    /// <summary>
    /// Fetches the homepage (website root) content item.
    /// </summary>
    /// <returns>The WebsiteRoot content item, or null if not found.</returns>
    /// <exception cref="ContentDeliveryException">Thrown when the Delivery API returns a server error (5xx).</exception>
    Task<IContentItem<WebsiteRoot>?> GetHomepageAsync();

    /// <summary>
    /// Fetches a page by its URL slug.
    /// </summary>
    /// <param name="slug">The URL-friendly slug of the page.</param>
    /// <returns>The Page content item, or null if not found.</returns>
    /// <exception cref="ContentDeliveryException">Thrown when the Delivery API returns a server error (5xx).</exception>
    Task<IContentItem<Page>?> GetPageBySlugAsync(string slug);
}
