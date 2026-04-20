using System.Diagnostics.CodeAnalysis;
using System.Net;
using Ficto.Generated.Models;
using Ficto.Models;
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
    /// the "default" collection, which holds content shared across all subsites.
    /// </summary>
    private IItemsFilterBuilder CollectionFilter(IItemsFilterBuilder b) =>
        b.System("collection").IsIn(CollectionCodename, "default");

    public async Task<IContentItem<WebsiteRoot>?> GetHomepageAsync(CancellationToken ct = default)
    {
        var result = await Client.GetItem<WebsiteRoot>(CollectionCodename)
            .Depth(3)
            .ExecuteAsync(ct);

        if (!result.IsSuccess)
        {
            if (result.StatusCode != HttpStatusCode.NotFound)
                LogAndThrow(result, "homepage");
            return null;
        }

        return result.Value;
    }

    public async Task<IContentItem<Page>?> GetPageBySlugAsync(string slug, CancellationToken ct = default)
    {
        var result = await Client.GetItems<Page>()
            .Where(i => i.Element("slug").IsEqualTo(slug))
            .Where(CollectionFilter)
            .Depth(3)
            .ExecuteAsync(ct);

        if (!result.IsSuccess)
        {
            if (result.StatusCode != HttpStatusCode.NotFound)
                LogAndThrow(result, "page", slug);
            return null;
        }

        return result.Value.Items.Count > 0 ? result.Value.Items[0] : null;
    }

    public async Task<PagedResult<IContentItem<Article>>> GetArticlesAsync(int skip = 0, int limit = 12, CancellationToken ct = default)
    {
        // Skip/Limit paginate at the API; WithTotalCount surfaces IPagination.TotalCount
        // so the view can render "Showing N-M of TOTAL" without a separate count query.
        // WithElements projects only the fields the listing cards need — the rich-text `content`
        // body is the heaviest field and is never rendered on a card, so we exclude it here.
        var result = await Client.GetItems<Article>()
            .Where(CollectionFilter)
            .OrderBy("elements.publishing_date", OrderingMode.Descending)
            .Skip(skip)
            .Limit(limit)
            .WithTotalCount()
            .WithElements(
                "title", "slug", "type", "abstract", "hero_image", "author", "publishing_date",
                "metadata__title", "metadata__description", "metadata__keywords")
            .ExecuteAsync(ct);

        if (!result.IsSuccess)
        {
            if (result.StatusCode != HttpStatusCode.NotFound)
                LogAndThrow(result, "articles");
            return PagedResult<IContentItem<Article>>.Empty(skip, limit);
        }

        return new PagedResult<IContentItem<Article>>(
            result.Value.Items, result.Value.Pagination.TotalCount, skip, limit);
    }

    public async Task<IContentItem<Article>?> GetArticleBySlugAsync(string slug, CancellationToken ct = default)
    {
        var result = await Client.GetItems<Article>()
            .Where(i => i.Element("slug").IsEqualTo(slug))
            .Where(CollectionFilter)
            .ExecuteAsync(ct);

        if (!result.IsSuccess)
        {
            if (result.StatusCode != HttpStatusCode.NotFound)
                LogAndThrow(result, "article", slug);
            return null;
        }

        return result.Value.Items.Count > 0 ? result.Value.Items[0] : null;
    }

    public async Task<PagedResult<IContentItem<Product>>> GetProductsAsync(
        IReadOnlyCollection<string>? categoryCodenames = null,
        int skip = 0,
        int limit = 12,
        CancellationToken ct = default)
    {
        // WithElements projects only the fields the product card renders — SEO metadata is
        // only used on the detail page, so it's excluded from this listing query.
        var query = Client.GetItems<Product>()
            .Where(CollectionFilter)
            .Skip(skip)
            .Limit(limit)
            .WithTotalCount()
            .WithElements(
                "product_base__name", "slug", "product_base__description",
                "product_base__main_image", "category", "price");

        if (categoryCodenames is { Count: > 0 })
        {
            var codenames = categoryCodenames.ToArray();
            query = query.Where(i => i.Element("category").ContainsAny(codenames));
        }

        var result = await query.ExecuteAsync(ct);

        if (!result.IsSuccess)
        {
            if (result.StatusCode != HttpStatusCode.NotFound)
                LogAndThrow(result, "products");
            return PagedResult<IContentItem<Product>>.Empty(skip, limit);
        }

        return new PagedResult<IContentItem<Product>>(
            result.Value.Items, result.Value.Pagination.TotalCount, skip, limit);
    }

    public async Task<IReadOnlyList<IContentItem<Product>>> GetProductsByCategoryAsync(
        IReadOnlyCollection<string> categoryCodenames,
        int limit = 4,
        CancellationToken ct = default)
    {
        if (categoryCodenames.Count == 0) return [];

        // Related-products strip renders cards — same projection as the main listing.
        var result = await Client.GetItems<Product>()
            .Where(CollectionFilter)
            .Where(i => i.Element("category").ContainsAny(categoryCodenames.ToArray()))
            .Limit(limit)
            .WithElements(
                "product_base__name", "slug", "product_base__description",
                "product_base__main_image", "category", "price")
            .ExecuteAsync(ct);

        if (!result.IsSuccess)
        {
            if (result.StatusCode != HttpStatusCode.NotFound)
                LogAndThrow(result, "products-by-category");
            return [];
        }

        return result.Value.Items;
    }

    public async Task<IContentItem<Product>?> GetProductBySlugAsync(string slug, CancellationToken ct = default)
    {
        var result = await Client.GetItems<Product>()
            .Where(i => i.Element("slug").IsEqualTo(slug))
            .Where(CollectionFilter)
            .ExecuteAsync(ct);

        if (!result.IsSuccess)
        {
            if (result.StatusCode != HttpStatusCode.NotFound)
                LogAndThrow(result, "product", slug);
            return null;
        }

        return result.Value.Items.Count > 0 ? result.Value.Items[0] : null;
    }

    public async Task<IReadOnlyList<IContentItem<Solution>>> GetSolutionsAsync(CancellationToken ct = default)
    {
        var result = await Client.GetItems<Solution>()
            .Where(CollectionFilter)
            .ExecuteAsync(ct);

        if (!result.IsSuccess)
        {
            if (result.StatusCode != HttpStatusCode.NotFound)
                LogAndThrow(result, "solutions");
            return [];
        }

        return result.Value.Items;
    }

    public async Task<IContentItem<Solution>?> GetSolutionBySlugAsync(string slug, CancellationToken ct = default)
    {
        var result = await Client.GetItems<Solution>()
            .Where(i => i.Element("slug").IsEqualTo(slug))
            .Where(CollectionFilter)
            .ExecuteAsync(ct);

        if (!result.IsSuccess)
        {
            if (result.StatusCode != HttpStatusCode.NotFound)
                LogAndThrow(result, "solution", slug);
            return null;
        }

        return result.Value.Items.Count > 0 ? result.Value.Items[0] : null;
    }

    public async Task<ITaxonomyGroup?> GetProductCategoryTaxonomyAsync(CancellationToken ct = default)
    {
        var result = await Client.GetTaxonomy("product_category").ExecuteAsync(ct);

        if (!result.IsSuccess)
        {
            if (result.StatusCode != HttpStatusCode.NotFound)
                LogAndThrow(result, "taxonomy", "product_category");
            return null;
        }

        return result.Value;
    }

    public async Task<IReadOnlyList<IContentItem<NavigationItem>>> GetNavigationAsync(CancellationToken ct = default)
    {
        // Depth(3) reaches WebsiteRoot → container → top-level nav items → dropdown subitems.
        var result = await Client.GetItem<WebsiteRoot>(CollectionCodename)
            .Depth(3)
            .ExecuteAsync(ct);

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
