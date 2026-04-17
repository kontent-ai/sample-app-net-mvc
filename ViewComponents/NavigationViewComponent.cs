using Ficto.Models;
using Ficto.Models.Mappers;
using Ficto.Services.Content;
using Microsoft.AspNetCore.Mvc;

namespace Ficto.ViewComponents;

public class NavigationViewComponent(
    IContentService contentService,
    NavigationItemMapper navigationMapper) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var items = await contentService.GetNavigationAsync();
        var viewModels = new List<NavigationViewModel>();

        foreach (var item in items)
        {
            viewModels.Add(await navigationMapper.MapAsync(item));
        }

        return View(viewModels);
    }
}
