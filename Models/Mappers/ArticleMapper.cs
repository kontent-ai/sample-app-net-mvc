using Ficto.Generated.Models;
using Kontent.Ai.Delivery;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Mappers;

public class ArticleMapper(PersonMapper personMapper) : IAsyncMapper<Article, ArticleViewModel>
{
    public async Task<ArticleViewModel> MapAsync(Article source)
    {
        var content = await source.Content.ToHtmlAsync();

        var author = source.Author.OfType<IContentItem<Person>>().FirstOrDefault();
        var authorViewModel = author != null ? await personMapper.MapAsync(author.Elements) : null;

        return new ArticleViewModel
        {
            Title = source.Title,
            Slug = source.Slug,
            ArticleType = source.Type.FirstOrDefault()?.Codename ?? string.Empty,
            Abstract = source.Abstract,
            Content = content,
            HeroImage = AssetViewModel.From(source.HeroImage.FirstOrDefault()),
            Author = authorViewModel,
            MetadataTitle = source.MetadataTitle,
            MetadataDescription = source.MetadataDescription,
            MetadataKeywords = source.MetadataKeywords,
            PublishingDate = source.PublishingDate.Value
        };
    }
}
