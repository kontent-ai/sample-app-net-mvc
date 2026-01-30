using Ficto.Generated.Models;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Services.Content.Interfaces;

/// <summary>
/// Comprises all the methods of the content service layer. Split into partials for organization and compile time safety.
/// </summary>
/// <remarks>
/// Whenever a new data fetch method is added, edit its corresponding partial interface or add a new one.
/// </remarks>
public partial interface IContentService
{
    /// <summary>
    /// Fetches an article by its URL slug.
    /// </summary>
    /// <param name="slug">The URL-friendly slug of the article.</param>
    /// <returns>The Article content item, or null if not found or on error.</returns>
    Task<IContentItem<Article>?> GetArticleBySlugAsync(string slug);
}
