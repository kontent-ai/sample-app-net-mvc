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
    /// Temporary placeholder used to demonstrate wiring the content service into MVC.
    /// Replace with a real implementation that fetches an article from Kontent.ai.
    /// </summary>
    /// <returns>Placeholder text and current collection codename as defined in AppSettings.json.</returns>
    Task<string> GetArticleAsync();
}