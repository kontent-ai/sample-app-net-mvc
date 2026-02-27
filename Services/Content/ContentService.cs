using System.Net;
using Ficto.Generated.Models;
using Ficto.Services.Content.Interfaces;
using Kontent.Ai.Delivery.Abstractions;
using Microsoft.Extensions.Options;

namespace Ficto.Services.Content;

/// <summary>
/// Central content access layer that wraps the Kontent.ai Delivery SDK.
/// Returns null/empty for content not found; throws <see cref="ContentDeliveryException"/> on server errors.
/// </summary>
public class ContentService(
    ILogger<ContentService> logger,
    IDeliveryClient deliveryClient,
    IOptions<SiteOptions> siteOptions) : IContentService
{
    private readonly ILogger<ContentService> _logger = logger;
    private readonly IDeliveryClient _deliveryClient = deliveryClient;
    // NOTE: The website root codename currently matches the collection codename by coincidence.
    // These are separate concepts — this should be its own config value.
    private readonly string _collectionCodename = siteOptions.Value.CollectionCodename;

    public async Task<IContentItem<WebsiteRoot>?> GetHomepageAsync()
    {
        var result = await _deliveryClient.GetItem<WebsiteRoot>(_collectionCodename)
            .Depth(3)
            .ExecuteAsync();

        if (!result.IsSuccess)
        {
            LogAndThrowOnServerError(result, "Failed to load homepage");
            return null;
        }

        return result.Value;
    }

    public async Task<IContentItem<Page>?> GetPageBySlugAsync(string slug)
    {
        var result = await _deliveryClient.GetItem<Page>(slug).ExecuteAsync();

        if (!result.IsSuccess)
        {
            LogAndThrowOnServerError(result, "Failed to load page '{Slug}'", slug);
            return null;
        }

        return result.Value;
    }

    public async Task<IContentItem<Article>?> GetArticleBySlugAsync(string slug)
    {
        var result = await _deliveryClient.GetItem<Article>(slug).ExecuteAsync();

        if (!result.IsSuccess)
        {
            LogAndThrowOnServerError(result, "Failed to load article '{Slug}'", slug);
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
            LogAndThrowOnServerError(result, "Failed to load products");
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
            LogAndThrowOnServerError(result, "Failed to load product by slug '{Slug}'", slug);
            return null;
        }

        return result.Value.Items.Count > 0 ? result.Value.Items[0] : null;
    }

    public async Task<IReadOnlyList<NavigationItem>> GetNavigationAsync()
    {
        var result = await _deliveryClient.GetItems()
            .Where(i => i.System("collection").IsEqualTo(_collectionCodename))
            .Depth(3)
            .ExecuteAsync();

        if (!result.IsSuccess)
        {
            LogAndThrowOnServerError(result, "Failed to load navigation");
            return [];
        }

        // The root navigation contains a single "Header navigation" container item.
        // Unwrap it to return the actual top-level nav items.
        var root = result.Value.Items
            .OfType<IContentItem<WebsiteRoot>>().FirstOrDefault();

        var rootItem = root?.Elements.Navigation
            .OfType<IContentItem<NavigationItem>>().FirstOrDefault();

        return rootItem?.Elements.Subitems
            .OfType<IContentItem<NavigationItem>>()
            .Select(x => x.Elements)
            .ToList() ?? [];
    }

    private void LogAndThrowOnServerError<T>(IDeliveryResult<T> result, string messageTemplate, params object[] args)
    {
        var allArgs = args.Append(result.Error?.Message).Append(result.StatusCode).ToArray();
        _logger.LogWarning(messageTemplate + ": {Error} (Status: {StatusCode})", allArgs);

        if ((int)result.StatusCode >= 500)
            throw new ContentDeliveryException(result.StatusCode, result.Error);
    }
}
