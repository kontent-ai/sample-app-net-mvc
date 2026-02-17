using Ficto.Generated.Models;

namespace Ficto.Services.Content.Interfaces;

public partial interface IContentService
{
    Task<IReadOnlyList<NavigationItem>> GetNavigationAsync();
}
