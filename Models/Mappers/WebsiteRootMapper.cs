using Ficto.Generated.Models;

namespace Ficto.Models.Mappers;

public class WebsiteRootMapper(
    NavigationItemMapper navigationItemMapper,
    IPageBlockMapperFactory pageBlockMapperFactory,
    PageMapper pageMapper) : IAsyncMapper<WebsiteRoot, WebsiteRootViewModel>
{
    private readonly NavigationItemMapper _navigationItemMapper = navigationItemMapper;
    private readonly IPageBlockMapperFactory _pageBlockMapperFactory = pageBlockMapperFactory;
    private readonly PageMapper _pageMapper = pageMapper;

    public async Task<WebsiteRootViewModel> MapAsync(WebsiteRoot source)
    {
        var navigation = new List<NavigationViewModel>();
        foreach (var navItem in source.Navigation.OfType<NavigationItem>())
        {
            navigation.Add(await _navigationItemMapper.MapAsync(navItem));
        }

        var content = await _pageBlockMapperFactory.MapManyAsync(source.Content);

        var subpages = new List<PageViewModel>();
        foreach (var page in source.Subpages.OfType<Page>())
        {
            subpages.Add(await _pageMapper.MapAsync(page));
        }

        return new WebsiteRootViewModel
        {
            Title = source.Title,
            Navigation = navigation,
            Content = content,
            Subpages = subpages,
            MetadataTitle = source.MetadataTitle,
            MetadataDescription = source.MetadataDescription,
            MetadataKeywords = source.MetadataKeywords
        };
    }
}
