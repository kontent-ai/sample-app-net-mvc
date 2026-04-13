using Microsoft.AspNetCore.Mvc;
using Ficto.Models.Mappers;
using Ficto.Services.Content.Interfaces;

namespace Ficto.Controllers;

public class PageController(IContentService contentService, PageMapper pageMapper) : Controller
{
    private readonly IContentService _contentService = contentService;
    private readonly PageMapper _pageMapper = pageMapper;

    public async Task<IActionResult> Index(string slug)
    {
        var page = await _contentService.GetPageBySlugAsync(slug);
        if (page == null)
            return NotFound();

        var viewModel = await _pageMapper.MapAsync(page.Elements);
        return View(viewModel);
    }
}
