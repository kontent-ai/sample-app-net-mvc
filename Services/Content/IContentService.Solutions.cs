using Ficto.Generated.Models;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Services.Content;

public partial interface IContentService
{
    /// <summary>
    /// Fetches all solutions.
    /// </summary>
    /// <returns>A list of Solution content items, or an empty list if none found.</returns>
    /// <exception cref="ContentDeliveryException">Thrown when the Delivery API returns a server error (5xx).</exception>
    Task<IReadOnlyList<IContentItem<Solution>>> GetSolutionsAsync();

    /// <summary>
    /// Fetches a solution by its URL slug.
    /// </summary>
    /// <param name="slug">The URL-friendly slug of the solution.</param>
    /// <returns>The Solution content item, or null if not found.</returns>
    /// <exception cref="ContentDeliveryException">Thrown when the Delivery API returns a server error (5xx).</exception>
    Task<IContentItem<Solution>?> GetSolutionBySlugAsync(string slug);
}
