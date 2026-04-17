using Ficto.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ficto.ViewComponents;

public class ProductFiltersViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        IReadOnlyList<CategoryTermViewModel> categories,
        IReadOnlyList<string> selected)
    {
        var viewModel = new ProductFiltersViewModel
        {
            Categories = categories,
            HasSelection = selected.Count > 0
        };
        return View(viewModel);
    }
}
