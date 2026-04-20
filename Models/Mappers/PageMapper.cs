using Ficto.Generated.Models;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Mappers;

public class PageMapper(IPageBlockMapperFactory pageBlockMapperFactory) : IAsyncMapper<IContentItem<Page>, PageViewModel>
{
    public async Task<PageViewModel> MapAsync(IContentItem<Page> source)
    {
        var e = source.Elements;
        var content = await pageBlockMapperFactory.MapManyAsync(e.Content);

        return new PageViewModel
        {
            ItemId = source.System.Id,
            Title = e.Title,
            Slug = e.Slug,
            Content = content,
            MetadataTitle = e.MetadataTitle,
            MetadataDescription = e.MetadataDescription,
            MetadataKeywords = e.MetadataKeywords
        };
    }
}
