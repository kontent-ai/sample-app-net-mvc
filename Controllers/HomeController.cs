using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Ficto.Models;
using Ficto.Models.Mappers;
using Ficto.Services.Content.Interfaces;

namespace Ficto.Controllers;

public class HomeController(
    IContentService contentService,
    WebsiteRootMapper websiteRootMapper) : Controller
{
    private readonly IContentService _contentService = contentService;
    private readonly WebsiteRootMapper _websiteRootMapper = websiteRootMapper;

    public async Task<IActionResult> Index()
    {
        var homepage = await _contentService.GetHomepageAsync();

        if (homepage == null)
        {
            return View("Error", new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        var viewModel = await _websiteRootMapper.MapAsync(homepage.Elements);
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
