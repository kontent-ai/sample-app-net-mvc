using System.Diagnostics.CodeAnalysis;
using System.Net;
using Ficto.Generated.Models;
using Ficto.Services.Content.Interfaces;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Services.Content;

/// <summary>
/// Central content access layer that wraps the Kontent.ai Delivery SDK.
/// Returns null/empty when content is not found (404); throws <see cref="ContentDeliveryException"/> on all other errors.
/// </summary>
public class ContentService(
    ILogger<ContentService> logger,
    IDeliveryClientFactory clientFactory,
    ISpaceContext spaceContext,
    IPreviewContext previewContext) : IContentService
{
    // Resolved once at construction — no per-request factory calls or exception-based branching.
    private readonly IDeliveryClient _production = clientFactory.Get("production");
    private readonly IDeliveryClient? _preview = clientFactory.TryGet("preview");

    /// <summary>
    /// Selects the preview or production client based on the current request host.
    /// Falls back to production with a warning if the preview client is not configured.
    /// </summary>
    private IDeliveryClient Client
    {
        get
        {
            if (!previewContext.IsPreview) return _production;
            if (_preview is not null) return _preview;

            logger.LogWarning("Preview client is not configured — fill in DeliveryOptions:PreviewApiKey in appsettings.json. Serving production content.");
            return _production;
        }
    }

    /// <summary>
    /// The active collection/space codename for this request, set by <see cref="Ficto.Middleware.SpaceContextMiddleware"/>.
    /// The WebsiteRoot item codename matches the collection codename by design in the sample project.
    /// </summary>
    private string CollectionCodename => spaceContext.SpaceCodename;

    /// <summary>
    /// Returns a filter that scopes queries to the active space's collection plus
    /// the "common" collection, which holds content shared across all subsites.
    /// </summary>
    private IItemsFilterBuilder CollectionFilter(IItemsFilterBuilder b) =>
        b.System("collection").IsIn(CollectionCodename, "common");

    public async Task<IContentItem<WebsiteRoot>?> GetHomepageAsync()
    {
        var result = await Client.GetItem<WebsiteRoot>(CollectionCodename)
            .Depth(3)
            .ExecuteAsync();

        if (!result.IsSuccess)
        {
            if (result.StatusCode != HttpStatusCode.NotFound)
                LogAndThrow(result, "homepage");
            return null;
        }

        return result.Value;
    }

    public async Task<IContentItem<Page>?> GetPageBySlugAsync(string slug)
    {
        var result = await Client.GetItems<Page>()
            .Where(i => i.Element("slug").IsEqualTo(slug))
            .Where(CollectionFilter)
            .Depth(3)
            .ExecuteAsync();

        if (!result.IsSuccess)
        {
            if (result.StatusCode != HttpStatusCode.NotFound)
                LogAndThrow(result, "page", slug);
            return null;
        }

        return result.Value.Items.Count > 0 ? result.Value.Items[0] : null;
    }

    public async Task<IReadOnlyList<IContentItem<Article>>> GetArticlesAsync()
    {
        var result = await Client.GetItems<Article>()
            .Where(CollectionFilter)
            .ExecuteAsync();

        if (!result.IsSuccess)
        {
            if (result.StatusCode != HttpStatusCode.NotFound)
                LogAndThrow(result, "articles");
            return [];
        }

        return result.Value.Items;
    }

    public async Task<IContentItem<Article>?> GetArticleBySlugAsync(string slug)
    {
        var result = await Client.GetItems<Article>()
            .Where(i => i.Element("slug").IsEqualTo(slug))
            .Where(CollectionFilter)
            .ExecuteAsync();

        if (!result.IsSuccess)
        {
            if (result.StatusCode != HttpStatusCode.NotFound)
                LogAndThrow(result, "article", slug);
            return null;
        }

        return result.Value.Items.Count > 0 ? result.Value.Items[0] : null;
    }

    public async Task<IReadOnlyList<IContentItem<Product>>> GetProductsAsync()
    {
        var result = await Client.GetItems<Product>()
            .Where(CollectionFilter)
            .ExecuteAsync();

        if (!result.IsSuccess)
        {
            if (result.StatusCode != HttpStatusCode.NotFound)
                LogAndThrow(result, "products");
            return [];
        }

        return result.Value.Items;
    }

    public async Task<IContentItem<Product>?> GetProductBySlugAsync(string slug)
    {
        var result = await Client.GetItems<Product>()
            .Where(i => i.Element("slug").IsEqualTo(slug))
            .Where(CollectionFilter)
            .ExecuteAsync();

        if (!result.IsSuccess)
        {
            if (result.StatusCode != HttpStatusCode.NotFound)
                LogAndThrow(result, "product", slug);
            return null;
        }

        return result.Value.Items.Count > 0 ? result.Value.Items[0] : null;
    }

    public async Task<IReadOnlyList<IContentItem<Solution>>> GetSolutionsAsync()
    {
        var result = await Client.GetItems<Solution>()
            .Where(CollectionFilter)
            .ExecuteAsync();

        if (!result.IsSuccess)
        {
            if (result.StatusCode != HttpStatusCode.NotFound)
                LogAndThrow(result, "solutions");
            return [];
        }

        return result.Value.Items;
    }

    public async Task<IContentItem<Solution>?> GetSolutionBySlugAsync(string slug)
    {
        var result = await Client.GetItems<Solution>()
            .Where(i => i.Element("slug").IsEqualTo(slug))
            .Where(CollectionFilter)
            .ExecuteAsync();

        if (!result.IsSuccess)
        {
            if (result.StatusCode != HttpStatusCode.NotFound)
                LogAndThrow(result, "solution", slug);
            return null;
        }

        return result.Value.Items.Count > 0 ? result.Value.Items[0] : null;
    }

    public async Task<IReadOnlyList<NavigationItem>> GetNavigationAsync()
    {
        // Depth(3) reaches WebsiteRoot → container → top-level nav items → dropdown subitems.
        var result = await Client.GetItem<WebsiteRoot>(CollectionCodename)
            .Depth(3)
            .ExecuteAsync();

        if (!result.IsSuccess)
        {
            if (result.StatusCode != HttpStatusCode.NotFound)
                LogAndThrow(result, "navigation");
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

    [DoesNotReturn]
    private void LogAndThrow<T>(IDeliveryResult<T> result, string contentContext, string? slug = null)
    {
        logger.LogWarning(
            "Content delivery failed. Context: {ContentContext}, Slug: {Slug}, Error: {Error}, Status: {StatusCode}",
            contentContext, slug ?? "(none)", result.Error?.Message, result.StatusCode);

        throw new ContentDeliveryException(result.StatusCode, result.Error);
    }
}
