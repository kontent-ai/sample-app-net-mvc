using Ficto.Generated.Models;
using Ficto.Services.Content.Interfaces;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Services.Content;

/// <summary>
/// Central content access layer that wraps the Kontent.ai Delivery SDK.
/// Handles errors internally and returns domain types directly.
/// </summary>
public class ContentService(ILogger<ContentService> logger, IDeliveryClient deliveryClient) : IContentService
{
    private readonly ILogger<ContentService> _logger = logger;
    private readonly IDeliveryClient _deliveryClient = deliveryClient;

    public async Task<IContentItem<WebsiteRoot>?> GetHomepageAsync()
    {
        var result = await _deliveryClient.GetItem<WebsiteRoot>("ficto_healthtech")
            .Depth(3)
            .ExecuteAsync();

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to load homepage: {Error} (Status: {StatusCode})",
                result.Error?.Message, result.StatusCode);
            return null;
        }

        return result.Value;
    }

    public async Task<IContentItem<Article>?> GetArticleBySlugAsync(string slug)
    {
        var result = await _deliveryClient.GetItem<Article>(slug).ExecuteAsync();

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to load article '{Slug}': {Error} (Status: {StatusCode})",
                slug, result.Error?.Message, result.StatusCode);
            return null;
        }

        return result.Value;
    }

    public async Task<IReadOnlyList<IContentItem<Product>>> GetProductsAsync()
    {
        var result = await _deliveryClient.GetItems<Product>()
            .Where(i => i.System("type").IsEqualTo("product"))
            .ExecuteAsync();

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to load products: {Error} (Status: {StatusCode})",
                result.Error?.Message, result.StatusCode);
            return [];
        }

        return result.Value.Items;
    }

    public async Task<IContentItem<Product>?> GetProductBySlugAsync(string slug)
    {
        var result = await _deliveryClient.GetItems<Product>()
            .Where(i => i.Element("elements.slug").IsEqualTo(slug))
            .ExecuteAsync();

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to load product by slug '{Slug}': {Error} (Status: {StatusCode})",
                slug, result.Error?.Message, result.StatusCode);
            return null;
        }

        return result.Value.Items.Count > 0 ? result.Value.Items[0] : null;
    }
}
