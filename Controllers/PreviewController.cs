using Microsoft.AspNetCore.Mvc;

namespace Ficto.Controllers;

/// <summary>
/// Clears the signed <c>ficto_preview</c> cookie to exit preview mode.
/// Preview is turned <em>on</em> automatically by <see cref="Ficto.Middleware.SpaceContextMiddleware"/>
/// when a request carries a valid <c>?secret=</c> — there is no manual enable endpoint.
/// </summary>
[Route("preview")]
public class PreviewController(ILogger<PreviewController> logger) : Controller
{
    public const string CookieName = "ficto_preview";

    /// <summary>
    /// Cookie options for the preview cookie. <c>SameSite=None</c> + <c>Secure</c> is required
    /// for the cookie to be sent on cross-site iframe requests — e.g. when Kontent.ai
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

    [HttpGet("disable")]
    public IActionResult Disable(string? returnUrl)
    {
        Response.Cookies.Delete(CookieName);
        logger.LogInformation("Preview mode disabled.");
        return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl!) : Redirect("/");
    }
}
