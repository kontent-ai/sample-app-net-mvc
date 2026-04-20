using Microsoft.AspNetCore.Mvc;
using Ficto.Models;
using Ficto.Models.Mappers;
using Ficto.Services.Content;

namespace Ficto.Controllers;

public class ArticlesController(
    IContentService contentService,
    ArticleMapper articleMapper,
    PageMapper pageMapper) : Controller
{
    private const int PageSize = 12;

    public async Task<IActionResult> Index([FromQuery] int page = 1, CancellationToken ct = default)
    {
        var skip = Math.Max(0, (page - 1) * PageSize);

        var pageTask = contentService.GetPageBySlugAsync("articles", ct);
        var articlesTask = contentService.GetArticlesAsync(skip, PageSize, ct);
        await Task.WhenAll(pageTask, articlesTask);

        var pageItem = await pageTask;
        var articles = await articlesTask;

        var pageViewModel = pageItem != null ? await pageMapper.MapAsync(pageItem) : null;

        var articleViewModels = await Task.WhenAll(
            articles.Items.Select(articleMapper.MapAsync));

        var viewModel = new ArticleListingViewModel
        {
            HeaderContent = pageViewModel?.Content ?? [],
            Articles = articleViewModels,
            Pager = PagerViewModel.From(articles),
        };

        return View(viewModel);
    }

    [Route("[controller]/{slug}")]
    public async Task<IActionResult> Details(string slug, CancellationToken ct)
    {
        var article = await contentService.GetArticleBySlugAsync(slug, ct);
        if (article == null)
            return NotFound();

        var viewModel = await articleMapper.MapAsync(article);
        return View(viewModel);
    }
}
