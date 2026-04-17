[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]

# Kontent.ai ASP.NET MVC sample app
<!-- ABOUT THE PROJECT -->
## About The Project

A Kontent.ai sample ASP.NET Core MVC application running on **.NET 8**, built on the v19 Delivery SDK. It supersedes the [legacy .NET sample app](https://github.com/kontent-ai/sample-app-net) and doubles as a reference for the patterns the new SDK was designed around — keyed client registration, webhook-driven cache invalidation, rich-text resolution, iframe-ready preview, and [Smart Link](https://github.com/kontent-ai/smart-link) click-to-edit overlays.

It's based on the **Kontent.ai Ficto multisite** project — three brand subsites (Imaging, Healthtech, Surgical) served from a single deployment with shared navigation and a common content collection for cross-brand pages.

<!-- GETTING STARTED -->
## Getting Started

Follow these steps to get the app running locally.

### Prerequisites

- .NET SDK **8.0** or newer
- A Kontent.ai environment containing the Ficto multisite sample content
- The environment's **Environment ID** (required)
- A **Preview API key** (optional — needed to see unpublished drafts)
- A **Secure Access API key** (optional — needed if Secure Access is enabled on the environment)

### Installation

1. Clone this repository:

```bash
git clone https://github.com/kontent-ai/sample-app-net-mvc.git
cd sample-app-net-mvc
```

2. Restore .NET dependencies:

```bash
dotnet restore
```

### Configuration & secrets

Set your environment ID in `appsettings.json`:

- `DeliveryOptions:EnvironmentId` — the environment this app reads from.

Everything else is optional, but recommended for the feature it enables. Store secrets via user-secrets rather than in `appsettings.json` so they never get committed:

```bash
dotnet user-secrets init
dotnet user-secrets set "DeliveryOptions:PreviewApiKey"      "<preview-api-key>"
dotnet user-secrets set "DeliveryOptions:SecureAccessApiKey" "<secure-access-api-key>"
dotnet user-secrets set "PreviewOptions:Secret"              "<preview-shared-secret>"
dotnet user-secrets set "WebhookOptions:Secret"              "<webhook-signing-secret>"
```

| Setting | Required for | Notes |
|---|---|---|
| `DeliveryOptions:EnvironmentId` | Any content read | The only hard requirement. |
| `DeliveryOptions:PreviewApiKey` | Preview mode | Without it, preview requests silently fall back to production with a warning. |
| `DeliveryOptions:SecureAccessApiKey` | Secure Access | Only needed if the environment has Secure Access enabled. |
| `PreviewOptions:Secret` | Preview auto-enable | Ships as `mySecret` so preview URLs work out-of-the-box; override for anything reachable. An empty value logs a warning and admits any non-empty `?secret=`. |
| `WebhookOptions:Secret` | Webhook-driven cache invalidation | Required HMAC secret; webhook calls with mismatching `X-KC-Signature` are rejected. |

user-secrets values are merged into configuration at runtime and are scoped to your local user profile.

> [!CAUTION]
> Storing keys directly in `appsettings.json` is convenient but risks accidental commit. Prefer user-secrets (above) for local dev and environment variables / Key Vault / a secrets manager for deployed environments.

### Build and run

Trust the ASP.NET Core dev certificate (one-time, per machine) so HTTPS works without browser warnings:

```bash
dotnet dev-certs https --trust
```

Build and run:

```bash
dotnet build
dotnet run
```

The app is served at `https://localhost:7108` (HTTP on `:5107` redirects to HTTPS).

#### Switching between spaces in dev

The Ficto sample is a multisite setup with three spaces (`ficto_imaging`, `ficto_healthtech`, `ficto_surgical`). In production each space is reached via its own subdomain; in local dev, use the `?collection=` query parameter instead — it's recognised by `SpaceContextMiddleware` and persisted in the `ficto_space` cookie for subsequent navigation:

```
https://localhost:7108/?collection=ficto_imaging
https://localhost:7108/?collection=ficto_surgical
```

See [Configuring the Kontent.ai preview URL](#configuring-the-kontentai-preview-url) for how the same `?collection=` parameter is used to target preview iframes at a specific subsite.



<!-- USAGE EXAMPLES -->
## Usage

The app is a content-rendered website for the fictional "Ficto" brand — three subsites sharing a common backbone. It exists as a learning reference for integrating Kontent.ai with ASP.NET Core MVC.

### Multisite routing

`SpaceContextMiddleware` resolves the active space for each request in this priority order:

1. **Subdomain** — `ficto-imaging.example.com` → `ficto_imaging` (hyphens become underscores; the `preview.` prefix is stripped before resolution).
2. **Query string** — `?collection=ficto_imaging`, which also persists to the `ficto_space` cookie.
3. **Cookie** — `ficto_space` from a prior selection.
4. **Default** — the first entry in `SiteOptions:Spaces`.

Every content query is scoped to the active space's collection plus the shared `"default"` collection, so content that's intentionally cross-brand (e.g. the *About us* page) lives in one place but appears under every subsite.

### Content queries

All Delivery SDK access goes through `IContentService` (`Services/Content/ContentService.cs`). It:

- selects the preview or production named `IDeliveryClient` based on `IPreviewContext.IsPreview`,
- applies the active-space + `"default"` collection filter to every list and slug query,
- returns `null` for 404s and maps other failures to a `ContentDeliveryException` so controllers can stay terse.

URL resolution for content-item links (in navigation and rich text) is handled by `IRouteResolver` using the templates in `SiteOptions:RouteTemplates`:

| Content type | URL pattern |
|---|---|
| `page` | `/{slug}` |
| `article` | `/Articles/{slug}` |
| `product` | `/Products/{slug}` |
| `solution` | `/Solutions/{slug}` |

Add a template if you introduce a new content type; anything not listed falls back to `/{type}/{slug}`.

### Rich text

`RichTextResolver` wires a single `IHtmlResolver` into the HTML pipeline. Inline linked items (Fact, Action, Callout) render through component-specific templates; links to other items resolve through `IRouteResolver` so `<a href>` values always match the routing table above. Custom anchor handling turns in-document references into deep-link `#slug` targets so table-of-contents links work.

### Paging, filtering, and taxonomies

Listing pages (Articles, Products) paginate through the SDK's `Skip` / `Limit` / `WithTotalCount` and return a `PagedResult<T>` so the view can render "Showing N–M of TOTAL" without a second count query. Products filter additionally by taxonomy — category codenames from the query string are passed into `.Where(i => i.Element("category").ContainsAny(...))` against the `product_category` taxonomy group.

### Navigation

The header menu is driven from a `WebsiteRoot` item in the active space. `NavigationViewComponent` fetches it via `IContentService.GetNavigationAsync()`, which uses `GetItem<WebsiteRoot>(spaceCodename)` with `Depth(3)` — enough to reach the top-level container, its nav items, and any dropdown subitems.



## Preview mode

Preview mode switches the active `IDeliveryClient` to the preview-keyed instance so editors see unpublished drafts. A single query parameter &mdash; `?secret=<PreviewOptions:Secret>` &mdash; is what flips it on. The sample ships with `PreviewOptions:Secret = "mySecret"` so everything works out-of-the-box; override it in user-secrets for anything reachable.

### How it works

`SpaceContextMiddleware` runs on every request. If the request carries `?secret=` and the value matches `PreviewOptions:Secret` (compared with `CryptographicOperations.FixedTimeEquals`), the middleware:

1. Issues a signed `ficto_preview` cookie via `IPreviewTokenProtector` (HttpOnly, SameSite=None, Secure, 1-day expiry &mdash; required for cross-site iframe use from Kontent.ai).
2. 302-redirects to the same URL with `?secret=` stripped, so the token never leaks into rendered HTML or the editor's URL bar.

On subsequent requests the valid cookie alone keeps `IPreviewContext.IsPreview` on, `ContentService` routes reads through the `"preview"` named Delivery client (from `DeliveryOptions:PreviewApiKey`), and the green banner shows at the top of every page. If the preview client isn't configured, the app logs a warning and silently serves production content &mdash; drafts just won't appear, no hard failure.

To exit preview, click the banner's **Disable** link (`GET /preview/disable`), which clears the cookie.

### Configuring the Kontent.ai preview URL

Point Kontent.ai at your local app so its live-preview iframe loads the rendered pages.

1. In Kontent.ai, open **Environment Settings → Preview URLs**.
2. On the **Space domains** tab, set the domain for every space (`ficto_imaging`, `ficto_healthtech`, `ficto_surgical`) to:

   ```
   localhost:7108
   ```

   (Adjust the port if you've customised `applicationUrl` — see [Port overrides](#port-overrides) below.)

3. Switch to the **Preview URLs for content types** tab and configure the template for each content type the app renders:

   | Content type | Preview URL template |
   |---|---|
   | `website_root` | `https://{Space}?collection={Collection}&secret=mySecret` |
   | `page` | `https://{Space}/{URLslug}?collection={Collection}&secret=mySecret` |
   | `article` | `https://{Space}/articles/{URLslug}?collection={Collection}&secret=mySecret` |
   | `solution` | `https://{Space}/solutions/{URLslug}?collection={Collection}&secret=mySecret` |
   | `product` | `https://{Space}/products/{URLslug}?collection={Collection}&secret=mySecret` |

   `{Space}`, `{Collection}`, and `{URLslug}` are Kontent.ai macros &mdash; Kontent.ai expands them per item / collection at preview time. The `secret=mySecret` value must match `PreviewOptions:Secret`; override it in user-secrets and update the templates accordingly before using a shared environment.

Once the iframe loads any of these URLs, the middleware sets the cookie, strips the secret from the URL, and subsequent clicks inside the iframe stay in preview mode via the `SameSite=None; Secure` cookie.

#### Port overrides

The `:7108` / `:5107` pair is just the default in `Properties/launchSettings.json`. Each URL in `applicationUrl` declares its own scheme explicitly &mdash; the port isn't bound to HTTP or HTTPS by position:

```json
"applicationUrl": "https://localhost:7108;http://localhost:5107"
```

Change either port freely (or swap in different ones). The ASP.NET Core dev cert is bound to the hostname `localhost`, not to a specific port, so HTTPS keeps working on whatever port you pick. Update the **Space domains** in Kontent.ai to match whichever HTTPS port you've configured &mdash; the iframe must load over HTTPS because Kontent.ai itself is served over HTTPS.

### ⚠ Gating preview in production

A shared URL secret is fine for a sample app &mdash; it is **not** a substitute for real authorization. Anyone who learns the secret sees drafts. For any reachable deployment, put a real auth boundary in front of preview requests using one of these idiomatic ASP.NET patterns:

1. **Standard ASP.NET authentication middleware** &mdash; configure `AddAuthentication` / `AddAuthorization` with your IdP (OIDC, Entra ID, cookie auth, etc.) and short-circuit unauthenticated preview requests before `SpaceContextMiddleware` runs. For example:

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

2. **Edge rules** &mdash; Cloudflare Access, Azure Front Door rules, AWS Cognito, or plain HTTP basic auth at a reverse proxy can all gate preview requests before they ever hit the app. Works well when preview is exposed on a dedicated hostname (e.g. `preview.ficto.example.com`).

Layer either approach on top of the `?secret=` mechanism. The secret then serves as the "turn preview display on" toggle; the auth boundary decides who's allowed to flip it.

### Why the cookie is signed

The `ficto_preview` cookie's value is opaque ciphertext protected by `IDataProtectionProvider`. Without signing, a visitor could type `ficto_preview=enabled` in devtools and bypass the secret check entirely; with signing, a forged value fails `Unprotect` and the middleware ignores it. The payload itself is a constant &mdash; the cookie says "this browser has presented a valid secret," nothing more.

## Smart Link (click-to-edit overlays)

The app integrates the [Kontent.ai Smart Link SDK](https://github.com/kontent-ai/smart-link) so editors in preview mode can click on one of the decorated elements and jump straight to editing the content in question.

### How it's wired

- **Script include** — `_Layout.cshtml` renders `Views/Shared/_SmartLinkScript.cshtml` inside `<head>` when `IPreviewContext.IsPreview` is true, pulling `kontent-smart-link@5` from the jsDelivr CDN and calling `initializeOnLoad()`. Production pages never load the SDK.
- **Environment + language attributes** — `_Layout.cshtml` puts `data-kontent-environment-id` and `data-kontent-language-codename` on `<body>` so the SDK can read them from any descendant. The environment ID comes from `DeliveryOptions:EnvironmentId`; language is hard-coded to `default` (the Ficto sample is single-language).
- **Item ID in view models** — every view model that maps a content item exposes a `Guid? ItemId` property populated from the Delivery SDK's `IContentItem<T>.System.Id`. Views emit it as `data-kontent-item-id="@Model.ItemId"`; Razor's conditional-attribute rendering omits the attribute entirely when `ItemId` is `null`.
- **Element codenames in views** — views decorate field-rendering tags with `data-kontent-element-codename="<codename>"` using the element codenames from the generated models (`Generated/Models/*.cs`, e.g. `product_base__name`, `title`, `reference__label`).
- **Rich-text inline components** — `RichTextResolver` emits `data-kontent-component-id` on the root of each inline Fact / Action / Callout template so editors can click into components embedded inside a rich-text field. These attributes are harmless in production (the SDK never loads) so the resolver stays a pure singleton with no preview-state dependency.

Attribute hierarchy matches the SDK's contract:

```
<body data-kontent-environment-id="…" data-kontent-language-codename="default">
  …
  <section data-kontent-item-id="…">
    <h1 data-kontent-element-codename="title">…</h1>
    <img data-kontent-element-codename="main_image" … />
  </section>
  …
</body>
```

### Activating the overlays

- **Inside Kontent.ai live preview** — when the app is loaded in Kontent.ai's preview iframe, the SDK auto-activates via iframe messaging. Nothing to do beyond a correctly configured preview URL (see [Configuring the Kontent.ai preview URL](#configuring-the-kontentai-preview-url)).
- **Standalone browser tab** — after enabling preview, append `?ksl-enabled` to any URL to activate overlays outside the iframe. Useful for debugging since browser devtools are fully accessible.

### Extending the decoration

To add Smart Link support to a new content type:

1. Add a `Guid? ItemId { get; init; }` property to the view model.
2. Change the mapper's `TSource` from `T` (bare elements) to `IContentItem<T>` (wrapper), read data via `source.Elements`, and set `ItemId = source.System.Id`.
3. Update call sites to pass the wrapper instead of `.Elements`.
4. In the view, wrap the item's outer container with `data-kontent-item-id="@Model.ItemId"` and decorate each field-rendering tag with `data-kontent-element-codename="<element codename>"` (copy codenames from `Generated/Models/<Type>.cs`'s `[JsonPropertyName]` attributes).

## Webhook-driven cache invalidation

The app caches Delivery API responses via `Kontent.Ai.Delivery.Caching` (FusionCache backend). The `/webhooks/kontent` endpoint receives Kontent.ai webhook notifications, validates the `X-KC-Signature` HMAC against `WebhookOptions:Secret`, and invalidates the corresponding cache dependency keys.

### How the cascade works

The SDK does not traverse a dependency graph at invalidation time. Instead, every cached response is **tagged at write time** with a fan-out set of keys — for an item or item-list response that includes the response item codenames, every linked/modular-content item codename, every referenced asset id, every referenced taxonomy group codename, and the content type codename of every primary and modular-content item. Invalidating a single tag (e.g. `item_homepage`, `type_article`, `taxonomy_personas`) removes every cached entry that was tagged with it.

The synthetic listing-scope keys (`scope_items_list`, `scope_types_list`, `scope_taxonomies_list`) are the safety net for **list-membership changes** — events where a new item should now appear in a previously cached filter that was never tagged with the new item's codename.

### Invalidation matrix

The endpoint only acts on notifications with `delivery_slot == "published"`. Preview events are skipped because the preview client is not cached.

| `object_type` | `action` | Keys invalidated | Why |
|---|---|---|---|
| `content_item` | `published` | `item_<codename>` + `scope_items_list` | Could be first publish (membership shift) or republish — safe default. |
| `content_item` | `unpublished` | `item_<codename>` | Existing listings tagged with the codename are evicted; the item cannot newly appear in unrelated listings. |
| `content_item` | `metadata_changed` | `item_<codename>` + `scope_items_list` | Codename rename or collection move can shift filter membership. |
| `asset` | `created` | _(no-op)_ | The new asset isn't yet referenced by any cached item. |
| `asset` | `changed` / `metadata_changed` / `deleted` | `asset_<id>` | Items referencing the asset (asset element or rich-text inline image) carry the same tag. |
| `content_type` | `created` | `scope_types_list` | New type joins `GetTypes()` listings; no cached item could reference it yet. |
| `content_type` | `changed` / `deleted` | `type_<codename>` + `scope_types_list` | `type_<codename>` evicts the type definition **and** every cached item / item-list whose payload contains an item of that type (directly or via modular content / linked items / inline rich-text items). |
| `taxonomy` | `created` | `scope_taxonomies_list` | |
| `taxonomy` | `metadata_changed` / `deleted` | `taxonomy_<codename>` + `scope_taxonomies_list` | Items referencing terms in the group are tagged with the group codename and get evicted. |
| `taxonomy` | `term_created` / `term_changed` / `term_deleted` / `terms_moved` | `taxonomy_<group_codename>` (from `data.system.taxonomy_group`) | Same fan-out as above — every item using a term in this group is tagged with the group codename. |
| `language` | `created` / `changed` / `deleted` | **Full purge** via `IDeliveryCachePurger.PurgeAsync()` | No language-scope key exists; languages affect every variant of every cached entry. |

Unknown `object_type` values are ignored and logged at `Debug`. Any notification processed in a webhook batch can opt into the full purge — if a single language event is present in the payload, the entire request is handled as a purge.

### SDK version requirement

`content_type.changed` and `content_type.deleted` rely on `type_<codename>` being attached to item and item-list caches by the SDK's `DependencyTrackingContext.TrackItemType` fan-out. The local `Kontent.Ai.Delivery` reference must include this change — without it, only the `GetType()` cache is invalidated and dependent item caches silently go stale.

### Webhook payload reference

See [Webhooks reference](https://kontent.ai/learn/docs/webhooks/webhooks/net) for the canonical payload structure. Codenames are read from `notifications[].data.system.codename`; asset ids from `notifications[].data.system.id`; taxonomy term events read the parent group from `notifications[].data.system.taxonomy_group`.



<!-- CONTRIBUTING -->
## Contributing

For Contributing please see  <a href="./CONTRIBUTING.md">`CONTRIBUTING.md`</a> for more information.



<!-- LICENSE -->
## License

Distributed under the MIT License. See [`LICENSE.md`](./LICENSE.md) for more information.


<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://github.com/kontent-ai/Home/wiki/Checklist-for-publishing-a-new-OS-project#badges-->
[contributors-shield]: https://img.shields.io/github/contributors/kontent-ai/sample-app-net-mvc.svg?style=for-the-badge
[contributors-url]: https://github.com/kontent-ai/sample-app-net-mvc/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/kontent-ai/sample-app-net-mvc.svg?style=for-the-badge
[forks-url]: https://github.com/kontent-ai/sample-app-net-mvc/network/members
[stars-shield]: https://img.shields.io/github/stars/kontent-ai/sample-app-net-mvc.svg?style=for-the-badge
[stars-url]: https://github.com/kontent-ai/sample-app-net-mvc/stargazers
[issues-shield]: https://img.shields.io/github/issues/kontent-ai/sample-app-net-mvc.svg?style=for-the-badge
[issues-url]:https://github.com/kontent-ai/sample-app-net-mvc/issues
[license-shield]: https://img.shields.io/github/license/kontent-ai/sample-app-net-mvc.svg?style=for-the-badge
[license-url]:https://github.com/kontent-ai/sample-app-net-mvc/blob/main/LICENSE.md
[discussion-shield]: https://img.shields.io/discord/821885171984891914?color=%237289DA&label=Kontent%2Eai%20Discord&logo=discord&style=for-the-badge
[discussion-url]: https://discord.com/invite/SKCxwPtevJ
