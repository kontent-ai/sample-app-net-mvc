using Microsoft.AspNetCore.Mvc;
using Ficto.Models.Mappers;
using Ficto.Services.Content.Interfaces;

namespace Ficto.Controllers;

public class PageController(IContentService contentService, PageMapper pageMapper) : Controller
{
    public async Task<IActionResult> Index(string slug)
    {
        var page = await contentService.GetPageBySlugAsync(slug);
        if (page == null)
            return NotFound();

        var viewModel = await pageMapper.MapAsync(page);
        return View(viewModel);
    }
}
