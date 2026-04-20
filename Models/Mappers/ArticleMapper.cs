using Ficto.Generated.Models;
using Kontent.Ai.Delivery;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Mappers;

public class ArticleMapper(PersonMapper personMapper, IHtmlResolver htmlResolver) : IAsyncMapper<IContentItem<Article>, ArticleViewModel>
{
    public async Task<ArticleViewModel> MapAsync(IContentItem<Article> source)
    {
        var e = source.Elements;
        // Listing queries project the `content` element out (see ContentService.GetArticlesAsync);
        // this mapper is shared with the detail page, so guard against the null element.
        var content = e.Content is null
            ? string.Empty
            : await e.Content.ToHtmlAsync(htmlResolver);

        var author = e.Author.OfType<IContentItem<Person>>().FirstOrDefault();
        var authorViewModel = author != null ? await personMapper.MapAsync(author) : null;

        return new ArticleViewModel
        {
            ItemId = source.System.Id,
            Title = e.Title,
            Slug = e.Slug,
            ArticleType = e.Type.FirstOrDefault()?.Codename ?? string.Empty,
            Abstract = e.Abstract,
            Content = content,
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
