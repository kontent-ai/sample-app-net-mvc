using Microsoft.AspNetCore.Mvc;
using Ficto.Models;
using Ficto.Models.Mappers;
using Ficto.Services.Content;

namespace Ficto.Controllers;

public class SolutionsController(
    IContentService contentService,
    SolutionMapper solutionMapper,
    PageMapper pageMapper) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var page = await contentService.GetPageBySlugAsync("solutions", ct);
        var solutions = await contentService.GetSolutionsAsync(ct);

        var pageViewModel = page != null ? await pageMapper.MapAsync(page) : null;

        var solutionViewModels = await Task.WhenAll(
            solutions.Select(solutionMapper.MapAsync));

        var viewModel = new SolutionListingViewModel
        {
            HeaderContent = pageViewModel?.Content ?? [],
            Solutions = solutionViewModels
        };

        return View(viewModel);
    }

    [Route("[controller]/{slug}")]
    public async Task<IActionResult> Details(string slug, CancellationToken ct)
    {
        var solution = await contentService.GetSolutionBySlugAsync(slug, ct);
        if (solution == null)
        {
            return NotFound();
        }
        var viewModel = await solutionMapper.MapAsync(solution);
        return View(viewModel);
    }
}
