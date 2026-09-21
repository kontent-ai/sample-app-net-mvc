# Multisite routing

The Ficto sample is three brand subsites (`ficto_imaging`, `ficto_healthtech`, `ficto_surgical`) served from one deployment. Each subsite is a Kontent.ai **space** backed by a collection of the same codename.

## How the active space is resolved

`SpaceContextMiddleware` resolves the space for each request in this priority order:

1. **Subdomain** — `ficto-imaging.example.com` → `ficto_imaging` (hyphens become underscores; a `preview.` prefix is stripped first).
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

The subdomain route also works locally over **HTTP only**: `http://ficto-imaging.localhost:5107`, `http://ficto-healthtech.localhost:5107`, `http://ficto-surgical.localhost:5107`. Most operating systems resolve `*.localhost` to loopback per RFC 6761; older Windows setups may need hosts-file entries.

HTTPS is not available on those hostnames — the ASP.NET Core dev certificate is issued for `localhost`, not `*.localhost`. Kontent.ai's preview iframe requires HTTPS, so `?collection=` is the only option for [preview URLs](preview.md#configuring-the-kontentai-preview-url).

## Changing ports

`:7108` / `:5107` are the defaults in `Properties/launchSettings.json`. Each URL in `applicationUrl` declares its own scheme, so either port can change freely:

```json
"applicationUrl": "https://localhost:7108;http://localhost:5107"
```

The dev certificate is bound to the hostname, not the port, so HTTPS keeps working. If you change the HTTPS port, update the **Space domains** in Kontent.ai to match.
