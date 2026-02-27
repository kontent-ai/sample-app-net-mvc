using Ficto.Generated.Models;
using Ficto.Models.Helpers;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Mappers;

public class NavigationItemMapper(ReferenceMapper referenceMapper) : IAsyncMapper<NavigationItem, NavigationViewModel>
{
    private readonly ReferenceMapper _referenceMapper = referenceMapper;

    public async Task<NavigationViewModel> MapAsync(NavigationItem source)
    {
        var reference = _referenceMapper.Map(new ReferenceInput(
            source.ReferenceLabel,
            source.ReferenceCaption,
            source.ReferenceExternalUri,
            source.ReferenceContentItemLink
        ));

        var subitems = new List<NavigationViewModel>();
        foreach (var subitem in source.Subitems.OfType<IContentItem<NavigationItem>>())
        {
            subitems.Add(await MapAsync(subitem.Elements));
        }

        return new NavigationViewModel
        {
            Label = source.ReferenceLabel,
            Url = RouteHelper.ResolveUrl(reference),
            IsExternal = reference is UrlReference,
            Subitems = subitems
        };
    }
}
