using Microsoft.AspNetCore.Mvc;
using Ficto.Models.Mappers;
using Ficto.Services.Content.Interfaces;

namespace Ficto.Controllers;

public class ProductsController(
    IContentService contentService,
    ProductMapper productMapper) : Controller
{
    private readonly IContentService _contentService = contentService;
    private readonly ProductMapper _productMapper = productMapper;

    // Convention: /Products or /Products/Index
    public async Task<IActionResult> Index()
    {
        var products = await _contentService.GetProductsAsync();
        var viewModels = products.Select(p => _productMapper.Map(p.Elements)).ToList();
        return View(viewModels);
    }

    // Route: /Products/{slug}
    [Route("[controller]/{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        var product = await _contentService.GetProductBySlugAsync(slug);

        if (product == null)
        {
            return NotFound();
        }

        var viewModel = _productMapper.Map(product.Elements);
        return View(viewModel);
    }

    // Attribute routing: /api/products
    [Route("api/products")]
    public async Task<IActionResult> ApiList()
    {
        var products = await _contentService.GetProductsAsync();

        var result = products.Select(p => new
        {
            name = p.Elements.ProductBaseName,
            slug = p.Elements.Slug,
            price = p.Elements.Price
        });

        return Json(result);
    }

    // Custom route: /p/{slug}
    [Route("p/{slug}")]
    public async Task<IActionResult> BySlug(string slug)
    {
        return await Details(slug);
    }
}
