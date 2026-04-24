using Ficto.Generated.Models;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Mappers;

public class SolutionMapper : IAsyncMapper<IContentItem<Solution>, SolutionViewModel>
{
    public Task<SolutionViewModel> MapAsync(IContentItem<Solution> source)
    {
        var e = source.Elements;

        return Task.FromResult(new SolutionViewModel
        {
            ItemId = source.System.Id,
            Slug = e.Slug,
            Name = e.ProductBaseName,
            Description = e.ProductBaseDescription,
            MainImage = e.ProductBaseMainImage.FirstOrDefault(),
            Showcase = e.Showcase,
            ImagingTechnology = e.ImagingTechnology.FirstOrDefault()?.Codename,
            MetadataTitle = e.MetadataTitle,
            MetadataDescription = e.MetadataDescription,
            MetadataKeywords = e.MetadataKeywords
        });
    }
}
