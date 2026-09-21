# Preview mode and Smart Link

- [Preview mode](#preview-mode)
  - [Configuring the Kontent.ai preview URL](#configuring-the-kontentai-preview-url)
  - [Gating preview in production](#gating-preview-in-production)
- [Smart Link (click-to-edit overlays)](#smart-link-click-to-edit-overlays)

## Preview mode

Preview mode switches the active `IDeliveryClient` to the preview-keyed instance so editors see unpublished drafts. One query parameter turns it on: `?secret=<PreviewOptions:Secret>`. The sample ships with `PreviewOptions:Secret = "mySecret"` so it works out of the box; override it in user-secrets for anything reachable.

`SpaceContextMiddleware` runs on every request. If the request carries `?secret=` and the value matches `PreviewOptions:Secret` (compared with `CryptographicOperations.FixedTimeEquals`), the middleware:

1. Issues a signed `ficto_preview` cookie via `IPreviewTokenProtector` (HttpOnly, SameSite=None, Secure, 1-day expiry — required for cross-site iframe use from Kontent.ai).
2. 302-redirects to the same URL with `?secret=` stripped, so the token never leaks into rendered HTML or the editor's URL bar.

On subsequent requests the valid cookie alone keeps `IPreviewContext.IsPreview` on, `ContentService` routes reads through the `"preview"` named Delivery client, and a green banner shows at the top of every page. If `DeliveryOptions:PreviewApiKey` is not set, the preview client is not registered: the app logs a warning and serves production content — drafts just don't appear, no hard failure.

To exit preview, click the banner's **Disable** link (`GET /preview/disable`), which clears the cookie.

> [!TIP]
> This is a showcase of the mechanism, not an access control. With an empty `PreviewOptions:Secret` the middleware logs a warning and accepts any non-empty `?secret=`. Real deployments put authorization in front of it — see [Gating preview in production](#gating-preview-in-production).

**Why the cookie is signed.** Its value is opaque ciphertext protected by `IDataProtectionProvider`. Without signing, a visitor could type `ficto_preview=enabled` in devtools and bypass the secret check; with signing, a forged value fails `Unprotect` and is ignored. The payload is a constant — the cookie says "this browser has presented a valid secret", nothing more.

### Configuring the Kontent.ai preview URL

Point Kontent.ai at your local app so its live-preview iframe loads the rendered pages.

1. In Kontent.ai, open **Environment settings → Preview URLs**.
2. On the **Space domains** tab, set the domain for every space (`ficto_imaging`, `ficto_healthtech`, `ficto_surgical`) to `localhost:7108` (or [your HTTPS port](multisite.md#changing-ports)).
3. On the **Preview URLs for content types** tab, configure a template for each content type the app renders:

   | Content type | Preview URL template |
   |---|---|
   | `website_root` | `https://{Space}?collection={Collection}&secret=mySecret` |
   | `page` | `https://{Space}/{URLslug}?collection={Collection}&secret=mySecret` |
   | `article` | `https://{Space}/articles/{URLslug}?collection={Collection}&secret=mySecret` |
   | `solution` | `https://{Space}/solutions/{URLslug}?collection={Collection}&secret=mySecret` |
   | `product` | `https://{Space}/products/{URLslug}?collection={Collection}&secret=mySecret` |

   `{Space}`, `{Collection}` and `{URLslug}` are Kontent.ai macros, expanded per item at preview time. `secret=` must match `PreviewOptions:Secret` — if you override the secret, update the templates. The paths mirror `SiteOptions:RouteTemplates`.

The iframe must load over HTTPS because Kontent.ai itself is served over HTTPS. Once it loads any of these URLs, the middleware sets the cookie and strips the secret, and clicks inside the iframe stay in preview via the `SameSite=None; Secure` cookie.

### Gating preview in production

A shared URL secret is fine for a sample app — it is **not** a substitute for real authorization. Anyone who learns the secret sees drafts. For any reachable deployment, put a real auth boundary in front of preview requests:

1. **Standard ASP.NET authentication middleware** — configure `AddAuthentication` / `AddAuthorization` with your IdP (OIDC, Entra ID, cookie auth, …) and short-circuit unauthenticated preview requests before `SpaceContextMiddleware` runs:

   ```csharp
   app.Use(async (ctx, next) =>
   {
       var entering = ctx.Request.Query.ContainsKey("secret");
       var inPreview = ctx.Request.Cookies.ContainsKey(PreviewController.CookieName);
       if ((entering || inPreview) && !(ctx.User.Identity?.IsAuthenticated ?? false))
       {
           await ctx.ChallengeAsync();
           return;
       }
       await next();
   });
   ```

2. **Edge rules** — Cloudflare Access, Azure Front Door rules, AWS Cognito, or HTTP basic auth at a reverse proxy can gate preview requests before they reach the app. Works well when preview is exposed on a dedicated hostname (e.g. `preview.ficto.example.com`); the app gives that label no special meaning, so the space then comes from `?collection=`.

Layer either on top of `?secret=`. The secret is then the "turn preview display on" toggle; the auth boundary decides who may flip it.

## Smart Link (click-to-edit overlays)

The app integrates the [Kontent.ai Smart Link SDK](https://github.com/kontent-ai/smart-link) so editors in preview mode can click a decorated element and jump straight to editing it.

### How it's wired

- **Script include** — `_Layout.cshtml` renders `Views/Shared/_SmartLinkScript.cshtml` inside `<head>` only when `IPreviewContext.IsPreview` is true, pulling `kontent-smart-link@5` from jsDelivr and calling `initializeOnLoad()`. Production pages never load the SDK.
- **Environment + language attributes** — `_Layout.cshtml` puts `data-kontent-environment-id` (from `DeliveryOptions:EnvironmentId`) and `data-kontent-language-codename` on `<body>`. Language is hard-coded to `default`; the Ficto sample is single-language.
- **Item ID in view models** — every view model that maps a content item exposes `Guid? ItemId`, populated from `IContentItem<T>.System.Id`. Views emit `data-kontent-item-id="@Model.ItemId"`; Razor omits the attribute when the value is `null`.
- **Element codenames in views** — field-rendering tags carry `data-kontent-element-codename="<codename>"`, using the codenames from `Generated/Models/*.cs` (e.g. `product_base__name`, `title`, `reference__label`).
- **Rich-text inline components** — `RichTextResolver` emits `data-kontent-component-id` on the root of each inline Fact / Action / Callout template. The attributes are harmless in production (the SDK never loads), so the resolver stays a singleton with no preview-state dependency.

The attribute hierarchy matches the SDK's contract:

```html
<body data-kontent-environment-id="…" data-kontent-language-codename="default">
  <section data-kontent-item-id="…">
    <h1 data-kontent-element-codename="title">…</h1>
    <img data-kontent-element-codename="main_image" … />
  </section>
</body>
```

### Activating the overlays

- **Inside Kontent.ai live preview** — the SDK auto-activates via iframe messaging. Nothing to do beyond a correctly configured [preview URL](#configuring-the-kontentai-preview-url).
- **Standalone browser tab** — after enabling preview, append `?ksl-enabled` to any URL. Useful for debugging, since browser devtools are fully accessible.

### Adding Smart Link support to a new content type

1. Add a `Guid? ItemId { get; init; }` property to the view model.
2. Change the mapper's `TSource` from `T` (bare elements) to `IContentItem<T>` (wrapper), read data via `source.Elements`, and set `ItemId = source.System.Id`.
3. Update call sites to pass the wrapper instead of `.Elements`.
4. In the view, put `data-kontent-item-id="@Model.ItemId"` on the item's outer container and `data-kontent-element-codename="<codename>"` on each field-rendering tag (codenames are the `[JsonPropertyName]` values in `Generated/Models/<Type>.cs`).
