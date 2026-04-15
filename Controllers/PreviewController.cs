using Ficto.Services.Content;
using Microsoft.AspNetCore.Mvc;

namespace Ficto.Controllers;

/// <summary>
/// Toggles preview mode on and off for the current browser. Enable delegates the authorization
/// decision to <see cref="IPreviewAccessGate"/>; the sample's default gate lets every request
/// through. Preview state lives in a signed cookie issued by <see cref="IPreviewTokenProtector"/>
/// so it can't be flipped on by hand-editing cookies.
/// </summary>
[Route("preview")]
public class PreviewController(
    IPreviewAccessGate gate,
    IPreviewTokenProtector protector,
    ILogger<PreviewController> logger) : Controller
{
    public const string CookieName = "ficto_preview";

    [HttpGet("enable")]
    public async Task<IActionResult> Enable(string? returnUrl, CancellationToken cancellationToken)
    {
        if (!await gate.CanEnableAsync(HttpContext, cancellationToken))
        {
            logger.LogInformation("Preview enable denied by {Gate}.", gate.GetType().Name);
            return Forbid();
        }

        Response.Cookies.Append(CookieName, protector.Issue(), new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(1),
            IsEssential = true,
        });

        logger.LogInformation("Preview mode enabled.");
        return LocalOrHome(returnUrl);
    }

    [HttpGet("disable")]
    public IActionResult Disable(string? returnUrl)
    {
        Response.Cookies.Delete(CookieName);
        logger.LogInformation("Preview mode disabled.");
        return LocalOrHome(returnUrl);
    }

    private RedirectResult LocalOrHome(string? returnUrl)
        => Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl!) : Redirect("/");
}
