using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace Ficto.Services.Rendering;

public sealed class RazorPartialRenderer(
    ICompositeViewEngine viewEngine,
    ITempDataProvider tempDataProvider,
    IHttpContextAccessor httpContextAccessor) : IPartialRenderer
{
    public async ValueTask<string> RenderAsync<TModel>(
        string partialName,
        TModel model,
        CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException(
                $"{nameof(RazorPartialRenderer)} requires an active HttpContext.");

        cancellationToken.ThrowIfCancellationRequested();

        var actionContext = new ActionContext(httpContext, httpContext.GetRouteData(), new ActionDescriptor());

        var viewResult = viewEngine.FindView(actionContext, partialName, isMainPage: false);
        if (!viewResult.Success)
        {
            var searched = string.Join(", ", viewResult.SearchedLocations);
            throw new InvalidOperationException(
                $"Partial view '{partialName}' was not found. Searched locations: {searched}");
        }

        await using var writer = new StringWriter();

        var viewData = new ViewDataDictionary<TModel>(
            new EmptyModelMetadataProvider(),
            new ModelStateDictionary())
        {
            Model = model
        };

        var viewContext = new ViewContext(
            actionContext,
            viewResult.View,
            viewData,
            new TempDataDictionary(httpContext, tempDataProvider),
            writer,
            new HtmlHelperOptions());

        await viewResult.View.RenderAsync(viewContext);
        return writer.ToString();
    }
}
