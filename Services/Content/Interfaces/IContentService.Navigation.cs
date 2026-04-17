using Ficto.Generated.Models;
using Ficto.Services.Content;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Services.Content.Interfaces;

public partial interface IContentService
{
    /// <summary>
    /// Fetches the top-level navigation items for the site.
    /// </summary>
    /// <returns>A list of navigation item wrappers (preserving System metadata), or an empty list if none found.</returns>
    /// <exception cref="ContentDeliveryException">Thrown when the Delivery API returns a server error (5xx).</exception>
    Task<IReadOnlyList<IContentItem<NavigationItem>>> GetNavigationAsync();
}
