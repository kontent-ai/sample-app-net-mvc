using System.Security.Cryptography;
using System.Text;
using Ficto.Configuration;
using Ficto.Controllers;
using Ficto.Services.Content;
using Microsoft.Extensions.Options;

namespace Ficto.Middleware;

/// <summary>
/// Resolves the active space/collection and preview flag for each request, storing them
/// in the scoped <see cref="SpaceContext"/> and <see cref="PreviewContext"/>.
///
/// Preview is driven by the signed <c>ficto_preview</c> cookie. Any request carrying
/// <c>?secret=&lt;PreviewOptions.Secret&gt;</c> has the cookie issued automatically and is
/// 302-redirected to strip the secret from the URL. This is what lets Kontent.ai Studio
/// load a target URL directly in its preview iframe — editors never click a manual
/// enable button. The secret comparison uses <see cref="CryptographicOperations.FixedTimeEquals"/>.
///
/// Space resolution priority:
/// <list type="number">
///   <item>Host subdomain</item>
///   <item>Query string <c>?collection=</c> — also sets a persistence cookie</item>
///   <item>Cookie <c>ficto_space</c></item>
///   <item>Default — first entry in <see cref="SiteOptions.Spaces"/></item>
/// </list>
/// </summary>
public class SpaceContextMiddleware(
    RequestDelegate next,
    IOptions<SiteOptions> siteOptions,
    IOptions<PreviewOptions> previewOptions,
    ILogger<SpaceContextMiddleware> logger)
{
    private const string CookieName = "ficto_space";
    private const string QueryParam = "collection";
    private const string PreviewSecretParam = "secret";

    private readonly PreviewOptions _previewOptions = previewOptions.Value;

    public async Task InvokeAsync(
        HttpContext context,
        SpaceContext spaceContext,
        PreviewContext previewContext,
        IPreviewTokenProtector previewTokenProtector)
    {
        var previewCookie = context.Request.Cookies[PreviewController.CookieName];
        previewContext.IsPreview = previewTokenProtector.IsValid(previewCookie);

        if (!previewContext.IsPreview
            && context.Request.Query.TryGetValue(PreviewSecretParam, out var supplied)
            && !string.IsNullOrEmpty(supplied)
            && SecretMatches(supplied.ToString()))
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

        spaceContext.SpaceCodename = ResolveSpace(context, context.Request.Host.Host, siteOptions.Value.Spaces);

        await next(context);
    }

    private bool SecretMatches(string supplied)
    {
        if (string.IsNullOrEmpty(_previewOptions.Secret))
        {
            logger.LogWarning(
                "Preview enable allowed without a secret — PreviewOptions:Secret is not configured. " +
                "Set it in user-secrets (dotnet user-secrets set PreviewOptions:Secret <value>) for any reachable deployment.");
            return true;
        }

        var expectedBytes = Encoding.UTF8.GetBytes(_previewOptions.Secret);
        var suppliedBytes = Encoding.UTF8.GetBytes(supplied);
        return CryptographicOperations.FixedTimeEquals(expectedBytes, suppliedBytes);
    }

    private static string ResolveSpace(HttpContext context, string host, string[] spaces)
    {
        // 1. Subdomain from host
        if (host.Contains('.'))
        {
            var subdomain = host.Split('.')[0].Replace('-', '_');
            if (spaces.Contains(subdomain))
                return subdomain;
        }

        // 2. Query string ?collection=ficto_imaging — also persists to cookie
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
