using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Ficto.Models;
using Ficto.Services.Content.Interfaces;

namespace Ficto.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IContentService _contentService;

    public HomeController(ILogger<HomeController> logger, IContentService contentService)
    {
        _logger = logger;
        _contentService = contentService;
    }

    public async Task<IActionResult> Index()
    {
        var articlePlaceholder = await _contentService.GetArticleAsync();
        ViewBag.ArticlePlaceholder = articlePlaceholder;
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
