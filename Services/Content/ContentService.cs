using Ficto.Generated.Models;
using Ficto.Services.Content.Interfaces;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Delivery.Configuration;
using Microsoft.Extensions.Options;

namespace Ficto.Services.Content;

/// <summary>
/// Central content access layer that wraps the Kontent.ai Delivery SDK.
/// Returns IDeliveryResult directly from the SDK for full access to response metadata.
/// </summary>
public class ContentService(ILogger<ContentService> logger, IDeliveryClient deliveryClient) : IContentService
{
    private readonly ILogger<ContentService> _logger = logger;
    private readonly IDeliveryClient _deliveryClient = deliveryClient;


    public Task<IDeliveryResult<IContentItem<WebsiteRoot>>> GetHomepageAsync()
    {
        return _deliveryClient.GetItem<WebsiteRoot>("ficto_healthtech")
            .Depth(3)
            .ExecuteAsync();
    }

    public Task<IDeliveryResult<IContentItem<Article>>> GetArticleBySlugAsync(string slug)
    {
        return _deliveryClient.GetItem<Article>(slug).ExecuteAsync();
    }
}
