using Microsoft.AspNetCore.Mvc;
using Ficto.Models;
using Ficto.Models.Mappers;
using Ficto.Services.Content.Interfaces;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Controllers;

public class ProductsController(
    IContentService contentService,
    ProductMapper productMapper,
    PageMapper pageMapper) : Controller
{
    private const int PageSize = 12;

    public async Task<IActionResult> Index(
        [FromQuery(Name = "category")] string[]? category,
        [FromQuery] int page = 1)
    {
        var selected = new HashSet<string>(
            category ?? [],
            StringComparer.OrdinalIgnoreCase);
        var skip = Math.Max(0, (page - 1) * PageSize);

        var taxonomyTask = contentService.GetProductCategoryTaxonomyAsync();
        var pageTask = contentService.GetPageBySlugAsync("products");
        var productsTask = contentService.GetProductsAsync(
            selected.Count > 0 ? selected : null,
            skip,
            PageSize);
        await Task.WhenAll(taxonomyTask, pageTask, productsTask);

        var taxonomy = await taxonomyTask;
        var pageItem = await pageTask;
        var products = await productsTask;

        var pageViewModel = pageItem != null ? await pageMapper.MapAsync(pageItem.Elements) : null;

        var productViewModels = await Task.WhenAll(
            products.Items.Select(p => productMapper.MapAsync(p.Elements)));

        var categories = taxonomy != null
            ? BuildCategoryTree(taxonomy.Terms, selected)
            : [];

        // Preserve the active-category filter across page links by emitting one query key
        // per selected category (so ASP.NET's model binder rebuilds the array on the next request).
        var extraQuery = selected
            .Select(c => new KeyValuePair<string, string>("category", c))
            .ToList();

        var viewModel = new ProductListingViewModel
        {
            HeaderContent = pageViewModel?.Content ?? [],
            Products = productViewModels,
            Categories = categories,
            SelectedCategories = [.. selected],
            Pager = PagerViewModel.From(products, extraQuery),
        };

        // AJAX requests from the filter form get just the product grid — the sidebar,
        // header content, and layout chrome are already in the DOM and don't need re-rendering.
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_ProductResults", viewModel);

        return View(viewModel);
    }

    [Route("[controller]/{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        var product = await contentService.GetProductBySlugAsync(slug);
        if (product == null)
            return NotFound();

        var productViewModel = await productMapper.MapAsync(product.Elements);

        var categoryCodenames = product.Elements.Category?
            .Select(t => t.Codename)
            .ToArray() ?? [];

        var related = await contentService.GetProductsByCategoryAsync(categoryCodenames, limit: 5);
        var relatedViewModels = await Task.WhenAll(
            related.Where(p => p.Elements.Slug != slug).Take(4)
                   .Select(p => productMapper.MapAsync(p.Elements)));

        return View(new ProductDetailViewModel
        {
            Product = productViewModel,
            Related = relatedViewModels,
        });
    }

    private static IReadOnlyList<CategoryTermViewModel> BuildCategoryTree(
        IReadOnlyList<ITaxonomyTermDetails> terms,
        HashSet<string> selected)
    {
        return terms.Select(t => new CategoryTermViewModel
        {
            Codename = t.Codename,
            Name = t.Name,
            IsSelected = selected.Contains(t.Codename),
            Children = BuildCategoryTree(t.Terms, selected),
        }).ToList();
    }
}
