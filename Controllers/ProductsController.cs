using Microsoft.AspNetCore.Mvc;
using Ficto.Models.Mappers;
using Ficto.Services.Content.Interfaces;

namespace Ficto.Controllers;

public class ProductsController(IContentService contentService, ProductMapper productMapper) : Controller
{
    private readonly IContentService _contentService = contentService;
    private readonly ProductMapper _productMapper = productMapper;

    public async Task<IActionResult> Index()
    {
        var products = await _contentService.GetProductsAsync();
        var viewModels = products.Select(p => _productMapper.Map(p.Elements)).ToList();
        return View(viewModels);
    }

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
}