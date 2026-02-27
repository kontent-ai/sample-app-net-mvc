using Ficto.Generated.Models;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Mappers;

public class PageMapper(IPageBlockMapperFactory pageBlockMapperFactory) : IAsyncMapper<Page, PageViewModel>
{
    private readonly IPageBlockMapperFactory _pageBlockMapperFactory = pageBlockMapperFactory;

    public async Task<PageViewModel> MapAsync(Page source)
    {
        var content = await _pageBlockMapperFactory.MapManyAsync(source.Content);

        var subpages = new List<PageViewModel>();
        foreach (var subpage in source.Subpages.OfType<IContentItem<Page>>())
        {
            subpages.Add(await MapAsync(subpage.Elements));
        }

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
