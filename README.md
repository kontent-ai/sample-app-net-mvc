> [!WARNING]
> This sample app is under active development. Details and documentation will evolve over time.

[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]

# Kontent.ai ASP.NET MVC sample app
<!-- ABOUT THE PROJECT -->
## About The Project

This is a Kontent.ai sample ASP.NET Core MVC application running on **.NET 8**.

It is based on the **Kontent.ai Ficto multisite** project. Once finalized, it will supersede the existing, [legacy .NET sample app](https://github.com/kontent-ai/sample-app-net).

<!-- GETTING STARTED -->
## Getting Started

Follow these steps to get the app running locally.

### Prerequisites

- .NET SDK **8.0** or newer
- A Kontent.ai Ficto multisite sample with:
  - Environment ID
  - (Optional) Preview API key
  - (Optional) Secure Access API key

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

1. Set your Kontent.ai environment ID in `appsettings.json`:
   - `DeliveryOptions.EnvironmentId` = your environment ID.

2. Optionally set preview or secure access keys:
> [!CAUTION]
> While for sample app purposes, storing keys directly in `appsettings.json` doesn't present any major risk, even if accidentally committed, consider using local secrets as described below.

```bash
dotnet user-secrets init
dotnet user-secrets set "DeliveryOptions:PreviewApiKey" "your-preview-api-key"
dotnet user-secrets set "DeliveryOptions:SecureAccessApiKey" "your-secure-access-api-key"
```

These values are kept in your local user profile and are not committed to the repository. They will become part of the corresponding `DeliveryOptions` section of `appSettings.json` during runtime.

### Build and run

Build the application:

```bash
dotnet build
```

Run the application:

```bash
dotnet run
```



<!-- USAGE EXAMPLES -->
## Usage

TBD



## Preview mode

The app ships with a working preview-mode toggle but **no authentication** on who can enable it — that's the sample's explicit responsibility to delegate to the consuming deployment.

### Turning preview on/off in the sample

- **Enable**: `GET /preview/enable?returnUrl=/Articles/<slug>` &mdash; sets a signed `ficto_preview` cookie (HttpOnly, SameSite=Lax, 1-day expiry), 302-redirects to `returnUrl` (local URLs only; external hosts fall back to `/`).
- **Disable**: `GET /preview/disable?returnUrl=<path>` &mdash; clears the cookie. The banner rendered in `_Layout.cshtml` links to this endpoint.

When the cookie is present and valid, `SpaceContextMiddleware` flips `IPreviewContext.IsPreview` on, `ContentService` routes reads through the `"preview"` named Delivery client (from `DeliveryOptions:PreviewApiKey`), and the green banner shows at the top of every page. If the preview client isn't configured, the app logs a warning and silently serves production content — drafts just won't appear, no hard failure.

### ⚠ The default gate is NOT a security boundary

`IPreviewAccessGate` is the authorization seam between an unauthenticated HTTP request and "you may turn preview on." The sample ships `AllowAnonymousPreviewAccessGate`, which always returns `true` and logs a warning every time it does. That means any visitor who hits `/preview/enable` sees drafts. This is fine for a local sample and a non-public demo; it is **not** fine for anything publicly reachable.

Replace the registration in `Program.cs` with your own implementation before deploying:

```csharp
services.AddSingleton<IPreviewAccessGate, MyAuthBackedGate>();
```

A plausible shape for a real gate (not implemented here — your auth system, your code):

```csharp
public sealed class OidcPreviewAccessGate(IAuthorizationService authz) : IPreviewAccessGate
{
    public async ValueTask<bool> CanEnableAsync(HttpContext ctx, CancellationToken ct)
    {
        var result = await authz.AuthorizeAsync(ctx.User, resource: null, policy: "CanPreviewContent");
        return result.Succeeded;
    }
}
```

Evolutions on the same theme: require a claim (`ctx.User.HasClaim("role", "editor")`), check an IP allow-list, require MFA, etc. The interface is deliberately minimal so it composes with whatever authz model you already run.

### Why the cookie is signed even without a secret

The `ficto_preview` cookie's value is opaque ciphertext protected by `IDataProtectionProvider`. Without signing, a visitor could type `ficto_preview=1` in devtools and bypass the gate entirely; with signing, a forged value fails `Unprotect` and the middleware ignores it. The payload itself is a constant &mdash; the cookie says "this browser has been approved," and the gate is the thing that decides who gets approved.

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
