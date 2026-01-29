using Microsoft.AspNetCore.Mvc;

namespace Ficto.Controllers;

public class ProductsController : Controller
{
    // Convention: /Products or /Products/Index
    public IActionResult Index()
    {
        var products = new[] { "Widget", "Gadget", "Gizmo" };
        return View(products);
    }

    // Convention: /Products/Details/5
    public IActionResult Details(int id)
    {
        return View(model: $"Product #{id}");
    }

    // Attribute routing: /api/products
    [Route("api/products")]
    public IActionResult ApiList()
    {
        return Json(new[] { "Widget", "Gadget" });
    }

    // Custom route: /p/{slug}
    [Route("p/{slug}")]
    public IActionResult BySlug(string slug)
    {
        return Content($"Looking up: {slug}");
    }

    // Route constraint: /Products/Item/123 (only integers)
    [Route("Products/Item/{id:int}")]
    public IActionResult Item(int id)
    {
        return Content($"Item ID: {id}");
    }
}
