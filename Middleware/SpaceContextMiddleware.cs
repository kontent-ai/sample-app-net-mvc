using Ficto.Services.Content;
using Microsoft.Extensions.Options;

namespace Ficto.Middleware;

/// <summary>
/// Resolves the active space/collection for each request and stores it in the scoped
/// <see cref="SpaceContext"/>. Resolution priority:
/// <list type="number">
///   <item>Host subdomain — <c>ficto-imaging.localhost</c> → <c>ficto_imaging</c> (preview URL routing)</item>
///   <item>Query string — <c>?space=ficto_imaging</c>, also sets the persistence cookie</item>
///   <item>Cookie — <c>ficto_space</c> plain-text value set by a previous query-string selection</item>
///   <item>Default — first entry in <see cref="SiteOptions.Spaces"/></item>
/// </list>
/// </summary>
public class SpaceContextMiddleware(RequestDelegate next, IOptions<SiteOptions> siteOptions)
{
    private const string CookieName = "ficto_space";
    private const string QueryParam = "space";

    public async Task InvokeAsync(HttpContext context, SpaceContext spaceContext)
    {
        spaceContext.SpaceCodename = Resolve(context, siteOptions.Value.Spaces);
        await next(context);
    }

    private static string Resolve(HttpContext context, string[] spaces)
    {
        // 1. Subdomain: ficto-imaging.localhost → ficto_imaging
        var host = context.Request.Host.Host;
        if (host.Contains('.'))
        {
            var subdomain = host.Split('.')[0].Replace('-', '_');
            if (spaces.Contains(subdomain))
                return subdomain;
        }

        // 2. Query string: ?space=ficto_imaging — also persists the choice to cookie
        if (context.Request.Query.TryGetValue(QueryParam, out var qs))
        {
            var fromQuery = qs.ToString();
            if (spaces.Contains(fromQuery))
            {
                context.Response.Cookies.Append(CookieName, fromQuery, new CookieOptions
                {
                    HttpOnly = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                });
                return fromQuery;
            }
        }

        // 3. Cookie
        if (context.Request.Cookies.TryGetValue(CookieName, out var fromCookie)
            && spaces.Contains(fromCookie))
            return fromCookie;

        // 4. Default
        return spaces.FirstOrDefault() ?? string.Empty;
    }
}
