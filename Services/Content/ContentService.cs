using Ficto.Services.Content.Interfaces;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Services.Content;

/// <summary>
/// Central content access layer. For now it only exposes placeholder methods that
/// will later be wired up to the Kontent.ai Delivery SDK.
/// </summary>
public class ContentService(ILogger<ContentService> logger) : IContentService
{
    private readonly ILogger<ContentService> _logger = logger;

    // TODO: Constructor inject delivery client first.
    // private readonly IDeliveryClient _deliveryClient = deliveryClient;

    public Task<string> GetArticleAsync()
    {
        // TODO: Replace this placeholder implementation with a real call to the
        // injected _deliveryClient once DeliveryOptions and models are in place,
        // e.g. fetching a strongly-typed article by codename or URL slug.
        _logger.LogInformation("GetArticleAsync called. Returning placeholder text until SDK integration is implemented.");
        return Task.FromResult("TODO: Add a check for each task in asana and render its completion here.");
    }
}