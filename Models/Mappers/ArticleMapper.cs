using Ficto.Generated.Models;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Mappers;

public class ArticleMapper(PersonMapper personMapper) : IAsyncMapper<IContentItem<Article>, ArticleViewModel>
{
    public async Task<ArticleViewModel> MapAsync(IContentItem<Article> source)
    {
        var e = source.Elements;
        var author = e.Author.OfType<IContentItem<Person>>().FirstOrDefault();
        var authorViewModel = author != null ? await personMapper.MapAsync(author) : null;

        return new ArticleViewModel
        {
            ItemId = source.System.Id,
            Title = e.Title,
            Slug = e.Slug,
            ArticleType = e.Type.FirstOrDefault()?.Codename ?? string.Empty,
            Abstract = e.Abstract,
            // Null when listing queries project `content` out via WithElements; guarded in views.
            Content = e.Content,
            // Rendition is applied by the SDK via DeliveryOptions:DefaultRenditionPreset.
            HeroImage = AssetViewModel.From(e.HeroImage.FirstOrDefault()),
            Author = authorViewModel,
            MetadataTitle = e.MetadataTitle,
            MetadataDescription = e.MetadataDescription,
            MetadataKeywords = e.MetadataKeywords,
            PublishingDate = e.PublishingDate.Value
        };
    }
}
