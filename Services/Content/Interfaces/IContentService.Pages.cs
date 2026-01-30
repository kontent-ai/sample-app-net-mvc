using Ficto.Generated.Models;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Services.Content.Interfaces;

public partial interface IContentService
{
    /// <summary>
    /// Fetches the homepage (website root) content item.
    /// </summary>
    /// <returns>The WebsiteRoot content item, or null if not found or on error.</returns>
    Task<IContentItem<WebsiteRoot>?> GetHomepageAsync();
}
