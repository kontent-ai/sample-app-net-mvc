using Ficto.Generated.Models;
using Ficto.Services.Content.Interfaces;
using Kontent.Ai.Delivery.Abstractions;
using Microsoft.Extensions.Options;

namespace Ficto.Services.Content;

/// <summary>
/// Central content access layer that wraps the Kontent.ai Delivery SDK.
/// Returns IDeliveryResult directly from the SDK for full access to response metadata.
/// </summary>
public class ContentService(ILogger<ContentService> logger, IDeliveryClient deliveryClient, IOptionsMonitor<SiteOptions> siteOptions) : IContentService
{
    private readonly ILogger<ContentService> _logger = logger;
    private readonly IDeliveryClient _deliveryClient = deliveryClient;

    // Inject the SiteOptions monitor to get the current site options.
    private readonly IOptionsMonitor<SiteOptions> _siteOptions = siteOptions;

    public Task<IDeliveryResult<IContentItem<Page>>> GetHomepageAsync()
    {
        var siteCollection = _siteOptions.CurrentValue.CollectionCodename;

        return _deliveryClient.GetItem<Page>("homepage")
        .InFilter("system.collection", new[] { siteCollection, "default"})
        .ExecuteAsync();
    }

    public Task<IDeliveryResult<IContentItem<Article>>> GetArticleBySlugAsync(string slug)
    {
        var siteCollection = _siteOptions.CurrentValue.CollectionCodename;

        return _deliveryClient.GetItem<Article>(slug)
        .InFilter("system.collection", new[] { siteCollection, "default"})
        .ExecuteAsync();
    }
    public Task <IDeliveryResult<IContentItem<T>>> GetItemAsync<T>(string codename)
    {
        var siteCollection = _siteOptions.CurrentValue.CollectionCodename;

        return _deliveryClient
        .GetItem<T>(codename)
        .InFilter("system.collection", new[] { siteCollection, "default"})
        .ExecuteAsync();
    }
}
