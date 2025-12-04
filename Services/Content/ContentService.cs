using Ficto.Generated.Models;
using Ficto.Services.Content.Interfaces;
using Kontent.Ai.Delivery.Abstractions;
using Microsoft.Extensions.Options;

namespace Ficto.Services.Content;

/// <summary>
/// Central content access layer that wraps the Kontent.ai Delivery SDK.
/// Returns IDeliveryResult directly from the SDK for full access to response metadata.
/// </summary>
public class ContentService(ILogger<ContentService> logger, IOptionsMonitor<SiteOptions> siteOptions, IDeliveryClient deliveryClient) : IContentService
{
    private readonly ILogger<ContentService> _logger = logger;
    private readonly IOptionsMonitor<SiteOptions> _siteContextOptions = siteOptions;
    private readonly IDeliveryClient _deliveryClient = deliveryClient;

    public Task<IDeliveryResult<IContentItem<T>>> GetItemAsync<T>(string codename) where T : class, IElementsModel
    {
        return _deliveryClient.GetItem<T>(codename).ExecuteAsync();
    }

    public Task<IDeliveryResult<IContentItem<Page>>> GetHomepageAsync()
    {
        return GetItemAsync<Page>("ficto_healthtech");
    }

    public Task<IDeliveryResult<IContentItem<Article>>> GetArticleBySlugAsync(string slug)
    {
        return GetItemAsync<Article>(slug);
    }
}
