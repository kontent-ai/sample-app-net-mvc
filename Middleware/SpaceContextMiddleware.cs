using Ficto.Services.Content;
using Microsoft.Extensions.Options;

namespace Ficto.Middleware;

/// <summary>
/// Resolves the active space/collection and preview flag for each request, storing them
/// in the scoped <see cref="SpaceContext"/> and <see cref="PreviewContext"/>.
///
/// Preview is determined purely by a <c>preview.</c> host prefix:
///   <c>preview.ficto-imaging.localhost</c> → IsPreview = true, space = ficto_imaging
///   <c>ficto-imaging.localhost</c>         → IsPreview = false, space = ficto_imaging
///
/// Space resolution priority (after stripping the preview prefix):
/// <list type="number">
///   <item>Host subdomain</item>
///   <item>Query string <c>?space=</c> — also sets a persistence cookie</item>
///   <item>Cookie <c>ficto_space</c></item>
///   <item>Default — first entry in <see cref="SiteOptions.Spaces"/></item>
/// </list>
/// </summary>
public class SpaceContextMiddleware(RequestDelegate next, IOptions<SiteOptions> siteOptions)
{
    private const string CookieName = "ficto_space";
    private const string QueryParam = "space";
    private const string PreviewPrefix = "preview.";

    public async Task InvokeAsync(HttpContext context, SpaceContext spaceContext, PreviewContext previewContext)
    {
        var host = context.Request.Host.Host;

        previewContext.IsPreview = host.StartsWith(PreviewPrefix, StringComparison.OrdinalIgnoreCase);

        // Strip the preview. prefix before resolving the space subdomain
        var spaceHost = previewContext.IsPreview ? host[PreviewPrefix.Length..] : host;

        spaceContext.SpaceCodename = ResolveSpace(context, spaceHost, siteOptions.Value.Spaces);

        await next(context);
    }

    private static string ResolveSpace(HttpContext context, string host, string[] spaces)
    {
        // 1. Subdomain from host (preview prefix already stripped)
        if (host.Contains('.'))
        {
            var subdomain = host.Split('.')[0].Replace('-', '_');
            if (spaces.Contains(subdomain))
                return subdomain;
        }

        // 2. Query string ?space=ficto_imaging — also persists to cookie
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
