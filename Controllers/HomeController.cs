using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Ficto.Models;
using Ficto.Services.Content.Interfaces;

namespace Ficto.Controllers;

public class HomeController(ILogger<HomeController> logger, IContentService contentService) : Controller
{
    private readonly ILogger<HomeController> _logger = logger;
    private readonly IContentService _contentService = contentService;

    public async Task<IActionResult> Index()
    {
        var result = await _contentService.GetHomepageAsync();

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to load homepage: {Error} (Status: {StatusCode})",
                result.Error?.Message, result.StatusCode);
            return RedirectToAction("Error");
        }

        return View(result.Value.Elements);
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
