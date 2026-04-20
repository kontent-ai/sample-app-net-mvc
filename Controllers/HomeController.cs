using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Ficto.Models;
using Ficto.Models.Mappers;
using Ficto.Services.Content;

namespace Ficto.Controllers;

public class HomeController(
    IContentService contentService,
    WebsiteRootMapper websiteRootMapper) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var homepage = await contentService.GetHomepageAsync(ct);
        if (homepage == null)
            return NotFound();

        var viewModel = await websiteRootMapper.MapAsync(homepage);
        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Renders a user-friendly page for HTTP error status codes (404, 500, etc.).
    /// Registered via <c>UseStatusCodePagesWithReExecute</c> in Program.cs.
    /// </summary>
    [Route("/StatusCode/{code:int}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult HttpError(int code)
    {
        Response.StatusCode = code;
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            StatusCode = code
        });
    }

    /// <summary>
    /// Global exception handler. Program.cs registers this with <c>app.UseExceptionHandler</c>.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
