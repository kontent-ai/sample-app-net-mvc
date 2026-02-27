using Ficto.Generated.Models;
using Ficto.Services.Content;

namespace Ficto.Services.Content.Interfaces;

public partial interface IContentService
{
    /// <summary>
    /// Fetches the top-level navigation items for the site.
    /// </summary>
    /// <returns>A list of navigation items, or an empty list if none found.</returns>
    /// <exception cref="ContentDeliveryException">Thrown when the Delivery API returns a server error (5xx).</exception>
    Task<IReadOnlyList<NavigationItem>> GetNavigationAsync();
}
