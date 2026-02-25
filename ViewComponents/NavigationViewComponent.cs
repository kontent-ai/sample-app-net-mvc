using Ficto.Generated.Models;
using Ficto.Models;
using Ficto.Models.Mappers;
using Ficto.Services.Content.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ficto.ViewComponents;

public class NavigationViewComponent(
    IContentService contentService,
    NavigationItemMapper navigationMapper) : ViewComponent
{
    private readonly IContentService _contentService = contentService;
    private readonly NavigationItemMapper _navigationMapper = navigationMapper;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var items = await _contentService.GetNavigationAsync();
        var viewModels = new List<NavigationViewModel>();

        foreach (var item in items)
        {
            viewModels.Add(await _navigationMapper.MapAsync(item));
        }

        return View(viewModels);
    }
}
