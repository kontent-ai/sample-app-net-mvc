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
    public async Task<IActionResult> Index([FromQuery(Name = "category")] string[]? category)
    {
        var selected = new HashSet<string>(
            category ?? Array.Empty<string>(),
            StringComparer.OrdinalIgnoreCase);

        var taxonomy = await contentService.GetProductCategoryTaxonomyAsync();
        var page = await contentService.GetPageBySlugAsync("products");
        var products = await contentService.GetProductsAsync(
            selected.Count > 0 ? selected : null);

        var pageViewModel = page != null ? await pageMapper.MapAsync(page.Elements) : null;

        var productViewModels = new List<ProductViewModel>();
        foreach (var product in products)
        {
            productViewModels.Add(await productMapper.MapAsync(product.Elements));
        }

        var categories = taxonomy != null
            ? BuildCategoryTree(taxonomy.Terms, selected)
            : [];

        var viewModel = new ProductListingViewModel
        {
            HeaderContent = pageViewModel?.Content ?? [],
            Products = productViewModels,
            Categories = categories,
            SelectedCategories = selected.ToList()
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

        var relatedViewModels = new List<ProductViewModel>();
        if (categoryCodenames.Length > 0)
        {
            var related = await contentService.GetProductsAsync(categoryCodenames);
            foreach (var item in related.Where(p => p.Elements.Slug != slug).Take(4))
            {
                relatedViewModels.Add(await productMapper.MapAsync(item.Elements));
            }
        }

        return View(new ProductDetailViewModel
        {
            Product = productViewModel,
            Related = relatedViewModels
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
            Children = BuildCategoryTree(t.Terms, selected)
        }).ToList();
    }
}
