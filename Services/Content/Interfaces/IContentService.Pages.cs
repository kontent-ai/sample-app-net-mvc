using Ficto.Generated.Models;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Services.Content.Interfaces;

public partial interface IContentService
{
    /// <summary>
    /// Fetches the homepage (website root) content item.
    /// </summary>
    /// <returns>An IDeliveryResult containing the WebsiteRoot content item.</returns>
    Task<IDeliveryResult<IContentItem<WebsiteRoot>>> GetHomepageAsync();
}
