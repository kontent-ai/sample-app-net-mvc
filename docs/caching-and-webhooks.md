# Caching and webhook-driven invalidation

## What is cached

The **production** Delivery client caches API responses in memory through `Kontent.Ai.Delivery.Caching` (`delivery.UseMemoryCache(...)` in `Program.cs`), with fail-safe enabled. The **preview** client is deliberately uncached so editors see changes immediately.

Every entry expires after `SiteOptions:CacheExpirationSeconds` (default **60**). That expiry is the safety net for a missed or unconfigured webhook; once webhooks are wired up, raise it to avoid needless re-fetches — but keep it finite (see [Limitations](#limitations)).

How the SDK tags cached responses with dependency keys, and what each key evicts, is covered upstream in the Delivery SDK's [caching guide](https://github.com/kontent-ai/dotnet/blob/main/src/delivery/docs/caching-guide.md#dependency-tracking).

**Logging.** FusionCache logs a `call` and a `return` line at `Information` for every cache operation, and one page fans out into dozens of lookups, so `appsettings.json` raises the `ZiggyCreatures.Caching.Fusion` category to `Warning`. Fail-safe activations and factory errors are warnings and still show. To watch the cache work, set `Logging:LogLevel:ZiggyCreatures.Caching.Fusion` to `Information` in `appsettings.Development.json`.

## Registering the webhook

Kontent.ai dispatches webhooks from the public internet, so it cannot reach `localhost`. For local development, expose the app through a tunnel — [ngrok](https://ngrok.com/), [Cloudflare Tunnel](https://developers.cloudflare.com/cloudflare-one/connections/connect-networks/), or equivalent:

```bash
ngrok http https://localhost:7108
# → forwarding https://<random>.ngrok-free.app → https://localhost:7108
```

In Kontent.ai under **Environment settings → Webhooks**, create a webhook pointing at `https://<your-host>/webhooks/kontent`, and copy the signing key it generates into `WebhookOptions:Secret`:

```bash
dotnet user-secrets set "WebhookOptions:Secret" "<webhook-signing-secret>"
```

**Without a secret the endpoint is disabled.** The signature validator from `Kontent.Ai.AspNetCore` refuses to run with an empty secret, because it would admit forged requests. So that the sample still starts after a plain clone, `Program.cs` only registers the validator when a secret is configured; otherwise it logs a warning and `/webhooks/*` answers `404`, leaving the cache on time-based expiry alone.

## What the endpoint does

`UseWebhookSignatureValidator` verifies the `X-Kontent-ai-Signature` header (falling back to the legacy `X-KC-Signature`) against the secret and answers `401` before the controller sees an unsigned request.

`WebhooksController` then binds the body to `WebhookNotification` and hands the batch to `IDeliveryCacheManager.InvalidateAsync(notifications, client)`. The models, the notification-to-key mapping and the asset usage lookup all come from `Kontent.Ai.AspNetCore` — see its [webhooks documentation](https://github.com/kontent-ai/dotnet/tree/main/src/aspnetcore#webhooks) for the mapping, and the Kontent.ai [webhooks reference](https://kontent.ai/learn/docs/webhooks/webhooks/net) for the canonical payload. What this app adds:

**Filtering.** Before invalidating, the controller drops:

- notifications whose `environment_id` differs from the production client's — dependency keys carry no environment;
- `content_item` notifications outside the `published` delivery slot — the preview client is not cached. Assets, content types, taxonomies and languages are shared between slots and always count.

**Per object type:**

| `object_type` | Result |
|---|---|
| `content_item` | `item_<codename>` + items-list scope |
| `content_type` | `type_<codename>` + types-list scope + items-list scope |
| `taxonomy` | `taxonomy_<group>` + taxonomies-list scope + items-list scope |
| `asset` | `asset_<id>` (reaches rich-text usages) **plus** the item key of every item holding the asset in an asset element, resolved through the SDK's used-in lookup — which is why the Delivery client is passed in |
| `language` | Full purge via `IDeliveryCachePurger` — no language key exists, so one language event turns the whole batch into a purge |
| anything else | ignored |

**Status codes.** Kontent.ai retries any non-`2xx` response with backoff, so the status is how the endpoint asks for a retry:

| Status | When |
|---|---|
| `204` | Invalidation (or purge) completed, or nothing in the batch was relevant |
| `503` | `InvalidateAsync` / `PurgeAsync` returned `false` — the invalidation did not complete |
| `500` | An asset's used-in lookup failed (`DeliveryRequestException`); nothing was invalidated for that asset |
| `400` | The payload lacks a documented member and fails model binding |
| `401` | Missing or invalid signature |
| `404` | No `WebhookOptions:Secret` configured |

One deliberate exception: a used-in lookup that answers `404` means the asset was **deleted**. No retry would change that and its usages can no longer be resolved, so the controller falls back to a full purge and answers `204`.

## Limitations

- **Renames.** A notification carries only the *new* codename, so a response cached under the old key lives until it expires. This is the reason to keep `CacheExpirationSeconds` finite.
- **Freshness right after invalidation.** The Delivery CDN can serve the pre-change copy for a short while after the webhook arrives, and an ordinary read that follows caches whatever it gets.
