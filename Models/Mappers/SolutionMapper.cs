using Ficto.Generated.Models;
using Kontent.Ai.Delivery;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Mappers;

public class SolutionMapper(IHtmlResolver htmlResolver) : IAsyncMapper<Solution, SolutionViewModel>
{
    public async Task<SolutionViewModel> MapAsync(Solution source)
    {
        var showcase = await source.Showcase.ToHtmlAsync(htmlResolver);

        return new SolutionViewModel
        {
            Slug = source.Slug,
            Name = source.ProductBaseName,
            Description = source.ProductBaseDescription,
            MainImage = AssetViewModel.From(source.ProductBaseMainImage.FirstOrDefault()),
            Showcase = showcase,
            ImagingTechnology = source.ImagingTechnology.FirstOrDefault()?.Codename,
            MetadataTitle = source.MetadataTitle,
            MetadataDescription = source.MetadataDescription,
            MetadataKeywords = source.MetadataKeywords
        };
    }
}
