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
    // The website root item codename matches the collection/space codename by design in the sample project.
    // Step 2 of the implementation plan will make this per-request from the active space cookie.
    private readonly string _collectionCodename = siteOptions.Value.CollectionCodename;

    /// <summary>
    /// Returns a filter that scopes queries to the active space's collection plus
    /// the "common" collection, which holds content shared across all subsites.
    /// </summary>
    private IItemsFilterBuilder CollectionFilter(IItemsFilterBuilder b) =>
        b.System("collection").IsIn(_collectionCodename, "common");

    public async Task<IContentItem<WebsiteRoot>?> GetHomepageAsync()
    {
        var result = await deliveryClient.GetItem<WebsiteRoot>(_collectionCodename)
            .Depth(3)
            .ExecuteAsync();

        if (!result.IsSuccess)
        {
            LogAndThrowOnServerError(result, "homepage");
            return null;
        }

        return result.Value;
    }

    public async Task<IContentItem<Page>?> GetPageBySlugAsync(string slug)
    {
        var result = await deliveryClient.GetItems<Page>()
            .Where(i => i.Element("slug").IsEqualTo(slug))
            .Where(CollectionFilter)
            .Depth(3)
            .ExecuteAsync();

        if (!result.IsSuccess)
        {
            LogAndThrowOnServerError(result, "page", slug);
            return null;
        }

        return result.Value.Items.Count > 0 ? result.Value.Items[0] : null;
    }

    public async Task<IReadOnlyList<IContentItem<Article>>> GetArticlesAsync()
    {
        var result = await deliveryClient.GetItems<Article>()
            .Where(CollectionFilter)
            .ExecuteAsync();

        if (!result.IsSuccess)
        {
            LogAndThrowOnServerError(result, "articles");
            return [];
        }

        return result.Value.Items;
    }

    public async Task<IContentItem<Article>?> GetArticleBySlugAsync(string slug)
    {
        var result = await deliveryClient.GetItems<Article>()
            .Where(i => i.Element("slug").IsEqualTo(slug))
            .Where(CollectionFilter)
            .ExecuteAsync();

        if (!result.IsSuccess)
        {
            LogAndThrowOnServerError(result, "article", slug);
            return null;
        }

        return result.Value.Items.Count > 0 ? result.Value.Items[0] : null;
    }

    public async Task<IReadOnlyList<IContentItem<Product>>> GetProductsAsync()
    {
        var result = await deliveryClient.GetItems<Product>()
            .Where(CollectionFilter)
            .ExecuteAsync();

        if (!result.IsSuccess)
        {
            LogAndThrowOnServerError(result, "products");
            return [];
        }

        return result.Value.Items;
    }

    public async Task<IContentItem<Product>?> GetProductBySlugAsync(string slug)
    {
        var result = await deliveryClient.GetItems<Product>()
            .Where(i => i.Element("slug").IsEqualTo(slug))
            .Where(CollectionFilter)
            .ExecuteAsync();

        if (!result.IsSuccess)
        {
            LogAndThrowOnServerError(result, "product", slug);
            return null;
        }

        return result.Value.Items.Count > 0 ? result.Value.Items[0] : null;
    }

    public async Task<IReadOnlyList<IContentItem<Solution>>> GetSolutionsAsync()
    {
        var result = await deliveryClient.GetItems<Solution>()
            .Where(CollectionFilter)
            .ExecuteAsync();

        if (!result.IsSuccess)
        {
            LogAndThrowOnServerError(result, "solutions");
            return [];
        }

        return result.Value.Items;
    }

    public async Task<IContentItem<Solution>?> GetSolutionBySlugAsync(string slug)
    {
        var result = await deliveryClient.GetItems<Solution>()
            .Where(i => i.Element("slug").IsEqualTo(slug))
            .Where(CollectionFilter)
            .ExecuteAsync();

        if (!result.IsSuccess)
        {
            LogAndThrowOnServerError(result, "solution", slug);
            return null;
        }

        return result.Value.Items.Count > 0 ? result.Value.Items[0] : null;
    }

    public async Task<IReadOnlyList<NavigationItem>> GetNavigationAsync()
    {
        // Fetch the WebsiteRoot directly by its codename with Depth(2) to reach
        // WebsiteRoot → NavigationItem container → NavigationItem subitems.
        var result = await deliveryClient.GetItem<WebsiteRoot>(_collectionCodename)
            .Depth(2)
            .ExecuteAsync();

        if (!result.IsSuccess)
        {
            LogAndThrowOnServerError(result, "navigation");
            return [];
        }

        // The navigation element contains a single NavigationItem acting as the
        // "Header navigation" container. Unwrap it to return the actual top-level nav items.
        var rootItem = result.Value.Elements.Navigation
            .OfType<IContentItem<NavigationItem>>().FirstOrDefault();

        return rootItem?.Elements.Subitems
            .OfType<IContentItem<NavigationItem>>()
            .Select(x => x.Elements)
            .ToList() ?? [];
    }

    private void LogAndThrowOnServerError<T>(IDeliveryResult<T> result, string contentContext, string? slug = null)
    {
        logger.LogWarning(
            "Content delivery failed. Context: {ContentContext}, Slug: {Slug}, Error: {Error}, Status: {StatusCode}",
            contentContext, slug ?? "(none)", result.Error?.Message, result.StatusCode);

        if ((int)result.StatusCode >= 500)
            throw new ContentDeliveryException(result.StatusCode, result.Error);
    }
}
