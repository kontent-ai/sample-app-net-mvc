using Ficto.Generated.Models;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Mappers;

// TODO: confirm whether subpages are surfaced in any view. If unused, remove from view model + mapping.
public class PageMapper(IPageBlockMapperFactory pageBlockMapperFactory) : IAsyncMapper<IContentItem<Page>, PageViewModel>
{
    public async Task<PageViewModel> MapAsync(IContentItem<Page> source)
    {
        var e = source.Elements;
        var content = await pageBlockMapperFactory.MapManyAsync(e.Content);

        var subpages = await Task.WhenAll(
            e.Subpages.OfType<IContentItem<Page>>().Select(MapAsync));

        return new PageViewModel
        {
            ItemId = source.System.Id,
            Title = e.Title,
            Slug = e.Slug,
            Content = content,
            Subpages = subpages,
            MetadataTitle = e.MetadataTitle,
            MetadataDescription = e.MetadataDescription,
            MetadataKeywords = e.MetadataKeywords
        };
    }
}
