using Ficto.Generated.Models;

namespace Ficto.Models.Mappers;

public class WebsiteRootMapper(IPageBlockMapperFactory pageBlockMapperFactory)
    : IAsyncMapper<WebsiteRoot, WebsiteRootViewModel>
{
    public async Task<WebsiteRootViewModel> MapAsync(WebsiteRoot source)
    {
        var content = await pageBlockMapperFactory.MapManyAsync(source.Content);

        return new WebsiteRootViewModel
        {
            Title = source.Title,
            Content = content,
            MetadataTitle = source.MetadataTitle,
            MetadataDescription = source.MetadataDescription,
            MetadataKeywords = source.MetadataKeywords
        };
    }
}
