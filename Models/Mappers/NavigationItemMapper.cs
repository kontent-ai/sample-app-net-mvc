using Ficto.Generated.Models;
using Ficto.Models.Helpers;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Mappers;

public class NavigationItemMapper(ReferenceMapper referenceMapper, IRouteResolver routeResolver)
    : IAsyncMapper<IContentItem<NavigationItem>, NavigationViewModel>
{
    public async Task<NavigationViewModel> MapAsync(IContentItem<NavigationItem> source)
    {
        var e = source.Elements;
        var reference = referenceMapper.Map(new ReferenceInput(
            e.ReferenceLabel,
            e.ReferenceCaption,
            e.ReferenceExternalUri,
            e.ReferenceContentItemLink
        ));

        var subitems = new List<NavigationViewModel>();
        foreach (var subitem in e.Subitems.OfType<IContentItem<NavigationItem>>())
        {
            subitems.Add(await MapAsync(subitem));
        }

        return new NavigationViewModel
        {
            ItemId = source.System.Id,
            Label = e.ReferenceLabel,
            Url = routeResolver.ResolveUrl(reference),
            IsExternal = reference is UrlReference,
            Subitems = subitems
        };
    }
}
