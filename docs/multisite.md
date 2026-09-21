# Multisite routing

The Ficto sample is three brand subsites (`ficto_imaging`, `ficto_healthtech`, `ficto_surgical`) served from one deployment. Each subsite is a Kontent.ai **space** backed by a collection of the same codename.

## How the active space is resolved

`SpaceContextMiddleware` resolves the space for each request in this priority order:

1. **Subdomain** — the first host label: `ficto-imaging.example.com` → `ficto_imaging` (hyphens become underscores). A label that is not a known space falls through to the next step.
2. **Query string** — `?collection=ficto_imaging`, which also persists to the `ficto_space` cookie.
3. **Cookie** — `ficto_space` from a prior selection.
4. **Default** — the first entry in `SiteOptions:Spaces`.

Every content query is scoped to the active space's collection plus the shared `"default"` collection, so content that is intentionally cross-brand (e.g. the *About us* page) lives in one place but appears under every subsite.

## Switching spaces in local development

In production each space has its own subdomain. Locally, use the query parameter:

```
https://localhost:7108/?collection=ficto_imaging
https://localhost:7108/?collection=ficto_surgical
```

Subdomains are meant for deployed hosts. The app runs on HTTPS and redirects HTTP to it, so that `https://localhost:7108` can be registered as a Kontent.ai [preview URL](preview.md#configuring-the-kontentai-preview-url) — the preview iframe requires HTTPS. The ASP.NET Core dev certificate covers `localhost` only, not `*.localhost`, so `https://ficto-imaging.localhost:7108` resolves the right space but only after clicking through a certificate warning. Locally, `?collection=` is the practical route, and the only one that works in preview URLs.

## Changing ports

`:7108` / `:5107` are the defaults in `Properties/launchSettings.json`. Each URL in `applicationUrl` declares its own scheme, so either port can change freely:

```json
"applicationUrl": "https://localhost:7108;http://localhost:5107"
```

The dev certificate is bound to the hostname, not the port, so HTTPS keeps working. If you change the HTTPS port, update the **Space domains** in Kontent.ai to match.
