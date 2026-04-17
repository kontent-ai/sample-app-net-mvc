using Ficto.Generated.Models;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Mappers;

public class WebsiteRootMapper(IPageBlockMapperFactory pageBlockMapperFactory)
    : IAsyncMapper<IContentItem<WebsiteRoot>, WebsiteRootViewModel>
{
    public async Task<WebsiteRootViewModel> MapAsync(IContentItem<WebsiteRoot> source)
    {
        var e = source.Elements;
        var content = await pageBlockMapperFactory.MapManyAsync(e.Content);

        return new WebsiteRootViewModel
        {
            ItemId = source.System.Id,
            Title = e.Title,
            Content = content,
            MetadataTitle = e.MetadataTitle,
            MetadataDescription = e.MetadataDescription,
            MetadataKeywords = e.MetadataKeywords
        };
    }
}
