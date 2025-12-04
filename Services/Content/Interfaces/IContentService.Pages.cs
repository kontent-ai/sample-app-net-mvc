using Ficto.Generated.Models;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Services.Content.Interfaces;

public partial interface IContentService
{
    /// <summary>
    /// Fetches the homepage content item.
    /// </summary>
    /// <returns>An IDeliveryResult containing the Page content item.</returns>
    Task<IDeliveryResult<IContentItem<Page>>> GetHomepageAsync();
}
