using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Ficto.Models;
using Ficto.Models.Mappers;
using Ficto.Services.Content.Interfaces;

namespace Ficto.Controllers;

public class HomeController(
    ILogger<HomeController> logger,
    IContentService contentService,
    WebsiteRootMapper websiteRootMapper) : Controller
{
    private readonly ILogger<HomeController> _logger = logger;
    private readonly IContentService _contentService = contentService;
    private readonly WebsiteRootMapper _websiteRootMapper = websiteRootMapper;

    public async Task<IActionResult> Index()
    {
        var result = await _contentService.GetHomepageAsync();

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to load homepage: {Error} (Status: {StatusCode})",
                result.Error?.Message, result.StatusCode);

            return View("Error", new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorMessage = result.Error?.Message,
                StatusCode = (int)result.StatusCode,
                ErrorCode = result.Error?.ErrorCode,
                ContentRequestUrl = result.RequestUrl
            });
        }

        var viewModel = await _websiteRootMapper.MapAsync(result.Value.Elements);
        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    // This is global exception handler. Program.cs registers this with app.UseExceptionHandler("/Home/Error");
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
