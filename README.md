# Kontent.ai ASP.NET MVC sample app

A sample ASP.NET Core MVC site on **.NET 10**, built on the v20 [Kontent.ai Delivery SDK](https://github.com/kontent-ai/dotnet/tree/main/src/delivery) and the companion [`Kontent.Ai.AspNetCore`](https://github.com/kontent-ai/dotnet/tree/main/src/aspnetcore) package. It supersedes the [legacy .NET sample app](https://github.com/kontent-ai/sample-app-net) and doubles as a reference for the patterns the SDK was designed around: named client registration, webhook-driven cache invalidation, rich-text resolution, iframe-ready preview, and [Smart Link](https://github.com/kontent-ai/smart-link) click-to-edit overlays.

It renders the **Kontent.ai Ficto multisite** project — three brand subsites (Imaging, Healthtech, Surgical) served from one deployment, with shared navigation and a common collection for cross-brand pages.

## Getting started

**Prerequisites**

- .NET SDK **10.0** or newer
- A Kontent.ai environment with the Ficto content: create a new project and pick the **Ficto multisite** template from the sample project gallery. It provides the content types, taxonomies and items the app expects.

**Run it**

```bash
git clone https://github.com/kontent-ai/sample-app-net-mvc.git
cd sample-app-net-mvc
dotnet dev-certs https --trust   # one-time, per machine
dotnet run
```

Set `DeliveryOptions:EnvironmentId` in `appsettings.json` to your environment's ID (**Environment settings → General**) — it is the only required setting. The app is served at `https://localhost:7108` (HTTP on `:5107` redirects to HTTPS).

To switch subsites locally, append `?collection=ficto_imaging`, `?collection=ficto_healthtech` or `?collection=ficto_surgical`; the choice persists in a cookie. See [Multisite routing](docs/multisite.md).

## Configuration

Everything except the environment ID is optional and enables one feature. Keep secrets out of `appsettings.json`:

```bash
dotnet user-secrets init
dotnet user-secrets set "DeliveryOptions:PreviewApiKey"      "<preview-api-key>"
dotnet user-secrets set "DeliveryOptions:SecureAccessApiKey" "<secure-access-api-key>"
dotnet user-secrets set "PreviewOptions:Secret"              "<preview-shared-secret>"
dotnet user-secrets set "WebhookOptions:Secret"              "<webhook-signing-secret>"
```

For non-secret local overrides, use `appsettings.Development.json` — it is gitignored and loaded automatically by `dotnet run`. For deployed environments, use environment variables or a secrets manager.

| Setting | Enables | Notes |
|---|---|---|
| `DeliveryOptions:EnvironmentId` | Any content read | Required. |
| `DeliveryOptions:PreviewApiKey` | [Preview mode](docs/preview.md) | From **Project settings → API keys**. Without it the preview client is not registered and preview requests fall back to production content with a warning. |
| `DeliveryOptions:SecureAccessApiKey` | Secure Access | Only if the environment has Secure Access enabled. |
| `DeliveryOptions:DefaultRenditionPreset` | Image renditions | Ships as `"default"`; applied by the SDK to every asset URL. |
| `PreviewOptions:Secret` | Turning preview on | Ships as `mySecret` so preview URLs work out of the box; override for anything reachable. |
| `WebhookOptions:Secret` | [Webhook cache invalidation](docs/caching-and-webhooks.md) | Signing secret from **Environment settings → Webhooks**. Without it the app still starts, but `/webhooks/*` answers `404`. |
| `SiteOptions:Spaces` | [Multisite routing](docs/multisite.md) | Space/collection codenames; the first is the default. Defaults to the three Ficto spaces. |
| `SiteOptions:CacheExpirationSeconds` | Cache lifetime | Default `60`. Raise it once webhooks are wired up. |
| `SiteOptions:RouteTemplates` | URL patterns | See [URLs](#urls-and-routing). **Merges** with the defaults: `{ "article": "/blog/{slug}" }` overrides only `article`. |
| `ImageTransformationOptions:ResponsiveWidths` | `<img-asset>` `srcset` | The width ladder used by the tag helper. |

The `SiteOptions` section ships empty; defaults live on `Services/Content/SiteOptions.cs` and are validated on startup.

> [!NOTE]
> Before deploying anywhere reachable, constrain `AllowedHosts` (ships as `"*"`), override `PreviewOptions:Secret`, and read [Gating preview in production](docs/preview.md#gating-preview-in-production).

## How the app is built

Each entry names the file to start reading from.

### Content access

All Delivery SDK access goes through `IContentService` (`Services/Content/ContentService.cs`). It selects the `"production"` or `"preview"` named `IDeliveryClient` based on `IPreviewContext.IsPreview`, scopes every query to the active space's collection plus the shared `"default"` collection, returns `null`/empty for a 404, and maps every other failure to `ContentDeliveryException`. Clients are registered in `Program.cs`; only the production client is cached.

### Content models

The records in `Generated/Models/` are produced by [`Kontent.Ai.ModelGenerator`](https://github.com/kontent-ai/dotnet/tree/main/src/model-generator), pinned as a local tool in `.config/dotnet-tools.json`. Regenerate after a content model change:

```bash
dotnet tool restore
dotnet tool run KontentModelGenerator --environmentId "<environment-id>" --namespace "Ficto.Generated.Models" --outputdir "./Generated/Models" --nullability semantic
```

No type provider is generated or hand-written: `Kontent.Ai.Delivery.SourceGeneration` emits it at compile time from the `[ContentTypeCodename]` attributes. Generated files are overwritten on regeneration — extend a model in a separate `partial record` (see `SlugProviders.cs`), never in the generated file.

Generated records are never bound to views. Mappers in `Models/Mappers/` turn them into the view models in `Models/`.

### URLs and routing

`IRouteResolver` (`Services/Routing/RouteResolver.cs`) resolves content-item links in navigation and rich text from `SiteOptions:RouteTemplates`:

| Content type | Default URL pattern |
|---|---|
| `page` | `/{slug}` |
| `article` | `/articles/{slug}` |
| `product` | `/products/{slug}` |
| `solution` | `/solutions/{slug}` |

Add a template when you introduce a routable content type; anything unlisted falls back to `/{type}/{slug}`. `SpaceContextMiddleware` resolves the active subsite from subdomain, query string or cookie — see [Multisite routing](docs/multisite.md).

### Rich text

Rich-text elements reach Razor as `IRichTextContent` and render through the `<rich-text content="@Model.Content" />` tag helper (minimal example: `Views/Shared/_ContentChunk.cshtml`). `RichTextResolver` (`Services/Content/`) builds the single `IHtmlResolver` behind it: inline Fact / Action / Callout components get their own templates, content-item links go through `IRouteResolver`, and in-document anchors become `#slug` deep links.

### Listings, paging and taxonomies

Article and product listings page with `Skip` / `Limit` / `WithTotalCount` and return a `PagedResult<T>`, so the view renders "Showing N–M of TOTAL" without a count query. Products also filter by the `product_category` taxonomy via `ContainsAny`. List queries use `.WithElements(...)` to drop what a card never renders (the article body, product SEO metadata); the `*BySlugAsync` detail queries fetch everything.

### Images

View models expose `IAsset?` directly and views render it with the `<img-asset>` tag helper, which emits `srcset`/`sizes` from `ImageTransformationOptions:ResponsiveWidths`. Because `DeliveryOptions:DefaultRenditionPreset` is set, every asset URL already carries the editor-defined rendition. CSS `background-image` sites that cannot use a tag helper (e.g. `_VisualContainerHeroUnit.cshtml`) build URLs with `ImageUrlBuilder`.

### Navigation

`NavigationViewComponent` renders the header from the active space's `WebsiteRoot` item, fetched with `Depth(3)` — enough for the container, its items and one level of dropdown.

### Preview and Smart Link

`?secret=<PreviewOptions:Secret>` issues a signed cookie that switches reads to the uncached preview client; in preview, the Smart Link SDK is loaded and views carry the `data-kontent-*` attributes for click-to-edit. Setup of the Kontent.ai preview URLs, production gating and how to decorate a new content type: [Preview mode and Smart Link](docs/preview.md).

### Caching and webhooks

`/webhooks/kontent` verifies the Kontent.ai signature and invalidates exactly the cached responses a change affects, with a time-based expiry as the safety net. Registering the webhook, what each notification evicts and the endpoint's retry semantics: [Caching and webhook-driven invalidation](docs/caching-and-webhooks.md).

## Contributing

See [`CONTRIBUTING.md`](./CONTRIBUTING.md). Conventions for working in this codebase — for people and coding agents alike — are in [`AGENTS.md`](./AGENTS.md).

## License

Distributed under the MIT License. See [`LICENSE.md`](./LICENSE.md).
