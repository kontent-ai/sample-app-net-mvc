using Ficto.Generated.Models;

namespace Ficto.Models.Mappers;

public class PageMapper(IPageBlockMapperFactory pageBlockMapperFactory) : IAsyncMapper<Page, PageViewModel>
{
    private readonly IPageBlockMapperFactory _pageBlockMapperFactory = pageBlockMapperFactory;

    public async Task<PageViewModel> MapAsync(Page source)
    {
        var content = await _pageBlockMapperFactory.MapManyAsync(source.Content);

        return new PageViewModel
        {
            Title = source.Title,
            Slug = source.Slug,
            Content = content,
            MetadataTitle = source.MetadataTitle,
            MetadataDescription = source.MetadataDescription,
            MetadataKeywords = source.MetadataKeywords
        };
    }
}
