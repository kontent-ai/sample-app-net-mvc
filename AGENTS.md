# Working in this codebase

Conventions and gotchas for contributors and coding agents. The [README](README.md) explains what the app does; this file explains how to change it without breaking its patterns.

## Commands

```bash
dotnet build                      # no test project exists; a clean build is the check
dotnet run                        # https://localhost:7108, add ?collection=<space> to switch subsite
dotnet tool restore               # restores the pinned Kontent.Ai.ModelGenerator
dotnet tool run KontentModelGenerator --environmentId "<environment-id>" --namespace "Ficto.Generated.Models" --outputdir "./Generated/Models" --nullability semantic
```

## Where things live

| Path | What |
|---|---|
| `Program.cs` | All DI and pipeline wiring: named Delivery clients, cache, rich text, webhook validator |
| `Generated/Models/` | Generated Delivery models — **never edit**; `SlugProviders.cs` is the one hand-written file (partial extensions) |
| `Services/Content/ContentService.cs` | The only place that calls the Delivery SDK; interface split across `IContentService.*.cs` partials |
| `Services/Content/RichTextResolver.cs` | The single `IHtmlResolver` configuration |
| `Services/Routing/RouteResolver.cs` | Content type + slug → URL, from `SiteOptions:RouteTemplates` |
| `Middleware/SpaceContextMiddleware.cs` | Resolves the active space and preview state per request |
| `Models/Mappers/` | Generated model → view model (`Models/*ViewModel.cs`) |
| `Controllers/WebhooksController.cs` | Webhook-driven cache invalidation |
| `docs/` | Feature deep-dives: multisite, preview + Smart Link, caching + webhooks |

## Rules

- **Go through `IContentService`.** Controllers and view components never touch `IDeliveryClient`. New queries go in `ContentService` with a matching `IContentService.<Area>.cs` partial, must apply `CollectionFilter` (active space + `"default"`), take a `CancellationToken`, and follow the existing result handling: `null`/empty on 404, `LogAndThrow` for everything else.
- **Failures are results.** `ExecuteAsync` returns `IDeliveryResult<T>`; check `IsSuccess`. A `catch (HttpRequestException)` around it is dead code — only cancellation throws.
- **Never bind generated records to views.** Map to a view model in `Models/Mappers/`. Mappers take the `IContentItem<T>` wrapper (not bare `T`) so they can set `ItemId = source.System.Id` for Smart Link, and read data via `source.Elements`.
- **Mappers are registered as concrete types** (`services.AddScoped<ArticleMapper>()`), not by interface. A new mapper needs its own registration in `Program.cs`. New page blocks also need a case in `PageBlockMapperFactory`.
- **Use generated codename constants** (`Article.SlugCodename`) rather than retyped strings in new query code.
- **A new routable content type** needs: a `SiteOptions.RouteTemplates` default, an `ISlugProvider` partial in `SlugProviders.cs`, and a preview URL template (see `docs/preview.md`).
- **Views carry Smart Link attributes**: `data-kontent-item-id` on the item container, `data-kontent-element-codename` on each field-rendering tag. Keep them when editing markup.
- **Secrets stay in user-secrets.** `appsettings.json` ships with empty `PreviewApiKey` and `WebhookOptions:Secret`, and the app must keep starting that way.

## Gotchas

- **Linked items are wrapped.** The SDK materialises linked items and components as `IContentItem<T>`, not as `T`. Filtering an `IEnumerable<IEmbeddedContent>` must use the wrapper, or it silently matches nothing:

  ```csharp
  e.Subitems.OfType<IContentItem<NavigationItem>>()   // correct — then read .Elements
  e.Subitems.OfType<NavigationItem>()                 // compiles, matches nothing
  ```

  The same applies to `is` checks — unwrap via `IContentItem.Elements` first (see `ReferenceMapper`).
- **Stale models fail quietly.** A content type with no generated model logs warning `1408` and its linked items vanish from every `OfType<IContentItem<T>>()`. If content is missing from a page, check the log and regenerate.
- **Projected listings have empty elements.** List queries use `.WithElements(...)`; anything not listed (e.g. `Article.Content`) comes back as its default value on listing results. Views and mappers must tolerate that, and a new card field must be added to the projection.
- **The default space (`ficto_imaging`) has no articles.** An empty `/articles` means the wrong subsite, not a bug — try `?collection=ficto_surgical`.
- **The webhook validator refuses an empty secret** and would fail startup, so `Program.cs` registers it conditionally. Don't make the registration unconditional.
- **The preview client is optional and uncached.** `clientFactory.TryGet("preview")` may return `null`; never attach a cache to it.
- **Package versions move together.** The five `Kontent.Ai.Delivery*` / `Kontent.Ai.Urls` packages share one version, and `Kontent.Ai.Delivery.SourceGeneration` must equal `Kontent.Ai.Delivery`. All current packages are `net10.0` only. Upgrade guides live in the [kontent-ai/dotnet](https://github.com/kontent-ai/dotnet) monorepo under `src/<package>/docs/upgrade/`.

## Commits and pull requests

Short, lowercase, subject-only commit messages (see `git log`). Pull requests use `.github/PULL_REQUEST_TEMPLATE.md`.
