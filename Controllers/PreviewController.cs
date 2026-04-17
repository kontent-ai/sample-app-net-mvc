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

    /// <summary>
    /// Cookie options for the preview cookie. <c>SameSite=None</c> + <c>Secure</c> is required
    /// for the cookie to be sent on cross-site iframe requests — e.g. when Kontent.ai Studio
    /// embeds the app as a live preview. Browsers reject <c>SameSite=None</c> without <c>Secure</c>,
    /// and they reject <c>Secure</c> cookies on plain HTTP; the app runs HTTPS in dev and prod.
    /// </summary>
    public static CookieOptions BuildPreviewCookieOptions() => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.None,
        Expires = DateTimeOffset.UtcNow.AddDays(1),
        IsEssential = true,
    };

    [HttpGet("enable")]
    public async Task<IActionResult> Enable(string? returnUrl, CancellationToken cancellationToken)
    {
        if (!await gate.CanEnableAsync(HttpContext, cancellationToken))
        {
            logger.LogInformation("Preview enable denied by {Gate}.", gate.GetType().Name);
            return Forbid();
        }

        Response.Cookies.Append(CookieName, protector.Issue(), BuildPreviewCookieOptions());

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
