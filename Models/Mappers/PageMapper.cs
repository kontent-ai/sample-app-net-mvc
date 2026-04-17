using Ficto.Generated.Models;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Mappers;

// TODO: confirm whether subpages are surfaced in any view. If unused, remove from view model + mapping.
public class PageMapper(IPageBlockMapperFactory pageBlockMapperFactory) : IAsyncMapper<Page, PageViewModel>
{
    public async Task<PageViewModel> MapAsync(Page source)
    {
        var content = await pageBlockMapperFactory.MapManyAsync(source.Content);

        var subpages = await Task.WhenAll(
            source.Subpages.OfType<IContentItem<Page>>().Select(sp => MapAsync(sp.Elements)));

        return new PageViewModel
        {
            Title = source.Title,
            Slug = source.Slug,
            Content = content,
            Subpages = subpages,
            MetadataTitle = source.MetadataTitle,
            MetadataDescription = source.MetadataDescription,
            MetadataKeywords = source.MetadataKeywords
        };
    }
}
