using Ficto.Controllers;
using Ficto.Services.Content;
using Microsoft.Extensions.Options;

namespace Ficto.Middleware;

/// <summary>
/// Resolves the active space/collection and preview flag for each request, storing them
/// in the scoped <see cref="SpaceContext"/> and <see cref="PreviewContext"/>.
///
/// Preview is driven by the signed <c>ficto_preview</c> cookie that <see cref="PreviewController"/>
/// issues after <see cref="IPreviewAccessGate.CanEnableAsync"/> approves the request. The host's
/// <c>preview.</c> prefix is only used to resolve the space codename (e.g.
/// <c>preview.ficto-imaging.localhost</c> still means "imaging space") — it does NOT authorize
/// draft access on its own.
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
    private const string PreviewSecretParam = "secret";

    public async Task InvokeAsync(
        HttpContext context,
        SpaceContext spaceContext,
        PreviewContext previewContext,
        IPreviewTokenProtector previewTokenProtector,
        IPreviewAccessGate previewGate)
    {
        var host = context.Request.Host.Host;

        var previewCookie = context.Request.Cookies[PreviewController.CookieName];
        previewContext.IsPreview = previewTokenProtector.IsValid(previewCookie);

        // Auto-enable preview when the request carries ?secret=<token> and the gate approves.
        // This is what lets Kontent.ai Studio load a target URL directly in its preview iframe
        // — without this, editors would need a manual /preview/enable round-trip that can't fit
        // into a per-content-type preview URL template. We redirect to strip the token so it
        // doesn't leak into rendered HTML or the editor's URL bar.
        if (!previewContext.IsPreview
            && context.Request.Query.TryGetValue(PreviewSecretParam, out var secret)
            && !string.IsNullOrEmpty(secret)
            && await previewGate.CanEnableAsync(context, context.RequestAborted))
        {
            context.Response.Cookies.Append(
                PreviewController.CookieName,
                previewTokenProtector.Issue(),
                PreviewController.BuildPreviewCookieOptions());

            previewContext.IsPreview = true;

            var cleanedQuery = QueryString.Create(
                context.Request.Query.Where(kv => kv.Key != PreviewSecretParam)
                    .SelectMany(kv => kv.Value.Select(v => KeyValuePair.Create<string, string?>(kv.Key, v))));
            context.Response.Redirect(context.Request.PathBase + context.Request.Path + cleanedQuery);
            return;
        }

        // Strip the preview. prefix before resolving the space subdomain — the subdomain still
        // encodes which space the preview URL targets, independent of the preview flag above.
        var spaceHost = host.StartsWith(PreviewPrefix, StringComparison.OrdinalIgnoreCase)
            ? host[PreviewPrefix.Length..]
            : host;

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
