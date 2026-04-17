using Ficto.Generated.Models;
using Kontent.Ai.Delivery;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Mappers;

public class SolutionMapper(IHtmlResolver htmlResolver) : IAsyncMapper<IContentItem<Solution>, SolutionViewModel>
{
    public async Task<SolutionViewModel> MapAsync(IContentItem<Solution> source)
    {
        var e = source.Elements;
        var showcase = await e.Showcase.ToHtmlAsync(htmlResolver);

        return new SolutionViewModel
        {
            ItemId = source.System.Id,
            Slug = e.Slug,
            Name = e.ProductBaseName,
            Description = e.ProductBaseDescription,
            MainImage = AssetViewModel.From(e.ProductBaseMainImage.FirstOrDefault()),
            Showcase = showcase,
            ImagingTechnology = e.ImagingTechnology.FirstOrDefault()?.Codename,
            MetadataTitle = e.MetadataTitle,
            MetadataDescription = e.MetadataDescription,
            MetadataKeywords = e.MetadataKeywords
        };
    }
}
