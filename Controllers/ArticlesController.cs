using Microsoft.AspNetCore.Mvc;
using Ficto.Models;
using Ficto.Models.Mappers;
using Ficto.Services.Content.Interfaces;

namespace Ficto.Controllers;

public class ArticlesController(
    IContentService contentService,
    ArticleMapper articleMapper,
    PageMapper pageMapper) : Controller
{
    public async Task<IActionResult> Index()
    {
        var page = await contentService.GetPageBySlugAsync("articles");
        var articles = await contentService.GetArticlesAsync();

        var pageViewModel = page != null ? await pageMapper.MapAsync(page.Elements) : null;

        var articleViewModels = new List<ArticleViewModel>();
        foreach (var article in articles)
        {
            articleViewModels.Add(await articleMapper.MapAsync(article.Elements));
        }

        var viewModel = new ArticleListingViewModel
        {
            HeaderContent = pageViewModel?.Content ?? [],
            Articles = articleViewModels
        };

        return View(viewModel);
    }

    [Route("[controller]/{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        var article = await contentService.GetArticleBySlugAsync(slug);
        if (article == null)
            return NotFound();

        var viewModel = await articleMapper.MapAsync(article.Elements);
        return View(viewModel);
    }
}
