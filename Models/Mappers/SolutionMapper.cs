using Ficto.Generated.Models;
using Kontent.Ai.Delivery.Extensions;

namespace Ficto.Models.Mappers;

public class SolutionMapper : IAsyncMapper<Solution, SolutionViewModel>
{
    public async Task<SolutionViewModel> MapAsync(Solution source)
    {
        var showcase = await source.Showcase.ToHtmlAsync();

        return new SolutionViewModel
        {
            Slug = source.Slug,
            Name = source.ProductBaseName,
            Description = source.ProductBaseDescription,
            MainImage = source.ProductBaseMainImage.FirstOrDefault(),
            Showcase = showcase,
            ImagingTechnology = source.ImagingTechnology.Select(t => t.Codename).ToList(),
            MetadataTitle = source.MetadataTitle,
            MetadataDescription = source.MetadataDescription,
            MetadataKeywords = source.MetadataKeywords
        };
    }
}
