using Microsoft.AspNetCore.Mvc;
using Ficto.Models;
using Ficto.Models.Mappers;
using Ficto.Services.Content.Interfaces;

namespace Ficto.Controllers;

public class SolutionsController(
    IContentService contentService,
    SolutionMapper solutionMapper,
    PageMapper pageMapper) : Controller
{
    public async Task<IActionResult> Index()
    {
        var page = await contentService.GetPageBySlugAsync("solutions");
        var solutions = await contentService.GetSolutionsAsync();

        var pageViewModel = page != null ? await pageMapper.MapAsync(page.Elements) : null;

        var solutionViewModels = await Task.WhenAll(
            solutions.Select(s => solutionMapper.MapAsync(s.Elements)));

        var viewModel = new SolutionListingViewModel
        {
            HeaderContent = pageViewModel?.Content ?? [],
            Solutions = solutionViewModels
        };

        return View(viewModel);
    }

    [Route("[controller]/{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        var solution = await contentService.GetSolutionBySlugAsync(slug);
        if (solution == null)
        {
            return NotFound();
        }
        var viewModel = await solutionMapper.MapAsync(solution.Elements);
        return View(viewModel);
    }
}
