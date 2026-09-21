using System.Net;
using Kontent.Ai.AspNetCore.Webhooks;
using Kontent.Ai.AspNetCore.Webhooks.Models;
using Kontent.Ai.Delivery.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Ficto.Controllers;

/// <summary>
/// Receives Kontent.ai webhook notifications and invalidates relevant production cache entries.
/// Signature validation happens upstream in <c>UseWebhookSignatureValidator</c>; payload binding,
/// the notification-to-dependency-key mapping and asset usage lookups come from
/// <c>Kontent.Ai.AspNetCore</c>. Returns 204 once the invalidation is complete and a non-2xx
/// status when it is not, so Kontent.ai retries the notification.
/// </summary>
[ApiController]
public class WebhooksController(
    IOptionsMonitor<DeliveryOptions> deliveryOptions,
    ILogger<WebhooksController> logger) : ControllerBase
{
    private const string ProductionClientName = "production";

    [HttpPost("/webhooks/kontent")]
    public async Task<IActionResult> Receive(
        [FromBody] WebhookNotification payload,
        [FromKeyedServices(ProductionClientName)] IDeliveryCacheManager cacheManager,
        [FromKeyedServices(ProductionClientName)] IDeliveryClient client,
        CancellationToken ct)
    {
        // Dependency keys carry no environment, so only act on notifications for the environment
        // the production client reads. Compared as GUIDs — the option validates in any GUID format.
        var environmentId = Guid.Parse(deliveryOptions.Get(ProductionClientName).EnvironmentId);

        var relevant = payload.Notifications
            .Where(n => n.Message.EnvironmentId == environmentId)
            // The preview client is not cached, so a content item change in the preview slot has
            // nothing to invalidate. Assets, types, taxonomies and languages are shared between
            // the slots and always count.
            .Where(n => n.Message.ObjectType != WebhookObjectTypes.ContentItem
                || n.Message.DeliverySlot == WebhookDeliverySlots.Published)
            .ToList();

        if (relevant.Count == 0) return NoContent();

        // A language change can reach any cached response through fallbacks and has no dependency
        // key of its own, so a single language event turns the whole batch into a full purge.
        if (relevant.Any(n => n.Message.ObjectType == WebhookObjectTypes.Language))
            return await PurgeAsync(cacheManager, "language event", ct);

        // Asset notifications also resolve the items holding the asset in an asset element through the
        // used-in lookup, which is why the client is passed. A failed lookup throws
        // DeliveryRequestException and invalidates nothing for that asset; letting it propagate is a
        // 500, which is the retry a transient failure needs.
        bool invalidated;
        try
        {
            invalidated = await cacheManager.InvalidateAsync(relevant, client, ct);
        }
        catch (DeliveryRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // A deleted asset has no usages left to look up, and no retry will change that. The items
            // that held it cannot be resolved any more, so purge — a superset of what the batch needed.
            return await PurgeAsync(cacheManager, "asset no longer exists", ct);
        }

        logger.LogInformation(
            "Webhook processed {Count} notification(s); dependency key(s) besides resolved asset usages: {Keys}; completed: {Completed}.",
            relevant.Count, string.Join(", ", relevant.GetCacheDependencyKeys()), invalidated);

        return invalidated ? NoContent() : StatusCode(StatusCodes.Status503ServiceUnavailable);
    }

    private async Task<IActionResult> PurgeAsync(IDeliveryCacheManager cacheManager, string reason, CancellationToken ct)
    {
        if (cacheManager is not IDeliveryCachePurger purger)
        {
            logger.LogWarning(
                "Webhook requires a full purge ({Reason}) but cache manager does not implement {Interface} — cache may be stale.",
                reason, nameof(IDeliveryCachePurger));
            return NoContent();
        }

        var purged = await purger.PurgeAsync(cancellationToken: ct);
        logger.LogInformation("Webhook triggered full cache purge ({Reason}); completed: {Completed}.", reason, purged);
        return purged ? NoContent() : StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
}
