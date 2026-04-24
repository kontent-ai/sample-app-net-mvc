using System.Text.Json.Serialization;
using Kontent.Ai.Delivery.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Ficto.Controllers;

/// <summary>
/// Receives Kontent.ai webhook notifications and invalidates relevant production cache entries.
/// Signature validation happens upstream in <c>UseWebhookSignatureValidator</c>. Always returns
/// 200 OK after a valid request — parse/invalidation errors are logged, not surfaced, so
/// Kontent.ai does not retry on transient issues.
/// </summary>
[ApiController]
public class WebhooksController(ILogger<WebhooksController> logger) : ControllerBase
{
    private const string PublishedDeliverySlot = "published";

    [HttpPost("/webhooks/kontent")]
    public async Task<IActionResult> Receive(
        [FromBody] WebhookPayload? payload,
        [FromKeyedServices("production")] IDeliveryCacheManager? cacheManager,
        CancellationToken ct)
    {
        if (cacheManager is null)
        {
            logger.LogWarning("Webhook received but no cache manager is registered for the production client.");
            return Ok();
        }

        if (payload?.Notifications is not { Count: > 0 }) return Ok();

        var (keys, requiresPurge) = BuildInvalidationPlan(payload.Notifications);

        if (requiresPurge)
        {
            if (cacheManager is IDeliveryCachePurger purger)
            {
                await purger.PurgeAsync(cancellationToken: ct);
                logger.LogInformation("Webhook triggered full cache purge (language event).");
            }
            else
            {
                logger.LogWarning(
                    "Language webhook received but cache manager does not implement {Interface} — cache may be stale.",
                    nameof(IDeliveryCachePurger));
            }

            return Ok();
        }

        if (keys.Length > 0)
        {
            await cacheManager.InvalidateAsync(keys, ct);
            logger.LogInformation(
                "Webhook invalidated {Count} cache dependency key(s): {Keys}",
                keys.Length, string.Join(", ", keys));
        }

        return Ok();
    }

    // Maps each (object_type, action) notification to the canonical SDK dependency keys per the
    // matrix documented in README.md. `language.*` events bypass key-based invalidation and trigger
    // a full purge because no language-scope dependency key exists.
    private (string[] Keys, bool RequiresPurge) BuildInvalidationPlan(IReadOnlyList<WebhookNotification> notifications)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        var requiresPurge = false;

        foreach (var notification in notifications)
        {
            if (!string.Equals(notification.Message.DeliverySlot, PublishedDeliverySlot, StringComparison.OrdinalIgnoreCase))
                continue;

            var system = notification.Data.System;
            var objectType = notification.Message.ObjectType;
            var action = notification.Message.Action;

            switch (objectType)
            {
                case "content_item":
                    AppendContentItemKeys(keys, action, system);
                    break;

                case "asset":
                    AppendAssetKeys(keys, action, system);
                    break;

                case "content_type":
                    AppendContentTypeKeys(keys, action, system);
                    break;

                case "taxonomy":
                    AppendTaxonomyKeys(keys, action, system);
                    break;

                case "language":
                    requiresPurge = true;
                    break;

                default:
                    logger.LogDebug(
                        "Webhook notification ignored — unknown object_type {ObjectType} (action {Action}).",
                        objectType, action);
                    break;
            }
        }

        return ([.. keys], requiresPurge);
    }

    private static void AppendContentItemKeys(HashSet<string> keys, string action, WebhookSystem system)
    {
        if (string.IsNullOrEmpty(system.Codename)) return;

        switch (action)
        {
            case "published":
            case "metadata_changed":
                keys.Add($"item_{system.Codename}");
                keys.Add(DeliveryCacheDependencies.ItemsListScope);
                break;

            case "unpublished":
                // Unpublishing can only remove the item from listings it was already part of; those
                // listings are already tagged with item_<codename>, so the scope key is unnecessary.
                keys.Add($"item_{system.Codename}");
                break;
        }
    }

    private static void AppendAssetKeys(HashSet<string> keys, string action, WebhookSystem system)
    {
        if (string.IsNullOrEmpty(system.Id)) return;

        switch (action)
        {
            case "changed":
            case "metadata_changed":
            case "deleted":
                keys.Add($"asset_{system.Id}");
                break;

                // `created` is intentionally a no-op — a brand-new asset cannot be referenced by any
                // already-cached item.
        }
    }

    private static void AppendContentTypeKeys(HashSet<string> keys, string action, WebhookSystem system)
    {
        switch (action)
        {
            case "created":
                keys.Add(DeliveryCacheDependencies.TypesListScope);
                break;

            case "changed":
            case "deleted":
                if (!string.IsNullOrEmpty(system.Codename))
                    keys.Add($"type_{system.Codename}");
                keys.Add(DeliveryCacheDependencies.TypesListScope);
                break;
        }
    }

    private static void AppendTaxonomyKeys(HashSet<string> keys, string action, WebhookSystem system)
    {
        switch (action)
        {
            case "created":
                keys.Add(DeliveryCacheDependencies.TaxonomiesListScope);
                break;

            case "metadata_changed":
            case "deleted":
                if (!string.IsNullOrEmpty(system.Codename))
                    keys.Add($"taxonomy_{system.Codename}");
                keys.Add(DeliveryCacheDependencies.TaxonomiesListScope);
                break;

            case "term_created":
            case "term_changed":
            case "term_deleted":
            case "terms_moved":
                // Term events identify the parent group via `taxonomy_group`; the group codename
                // is the dependency tag attached to both the taxonomy listing cache and to every
                // item that references a term in this group.
                if (!string.IsNullOrEmpty(system.TaxonomyGroup))
                    keys.Add($"taxonomy_{system.TaxonomyGroup}");
                break;
        }
    }

    public record WebhookPayload(
        [property: JsonPropertyName("notifications")] IReadOnlyList<WebhookNotification> Notifications);

    public record WebhookNotification(
        [property: JsonPropertyName("data")] WebhookData Data,
        [property: JsonPropertyName("message")] WebhookMessage Message);

    public record WebhookData(
        [property: JsonPropertyName("system")] WebhookSystem System);

    public record WebhookSystem(
        [property: JsonPropertyName("id")] string? Id,
        [property: JsonPropertyName("codename")] string? Codename,
        [property: JsonPropertyName("taxonomy_group")] string? TaxonomyGroup);

    public record WebhookMessage(
        [property: JsonPropertyName("object_type")] string ObjectType,
        [property: JsonPropertyName("action")] string Action,
        [property: JsonPropertyName("delivery_slot")] string DeliverySlot);
}
