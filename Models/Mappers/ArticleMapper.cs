using Ficto.Generated.Models;
using Kontent.Ai.Delivery.Extensions;

namespace Ficto.Models.Mappers;

public class ArticleMapper(PersonMapper personMapper) : IAsyncMapper<Article, ArticleViewModel>
{
    private readonly PersonMapper _personMapper = personMapper;

    public async Task<ArticleViewModel> MapAsync(Article source)
    {
        var content = await source.Content.ToHtmlAsync();

        var authors = new List<PersonViewModel>();
        foreach (var author in source.Author.OfType<Person>())
        {
            authors.Add(await _personMapper.MapAsync(author));
        }

        return new ArticleViewModel
        {
            Title = source.Title,
            Slug = source.Slug,
            ArticleType = source.Type.FirstOrDefault()?.Codename ?? string.Empty,
            Abstract = source.Abstract,
            Content = content,
            HeroImage = source.HeroImage.FirstOrDefault(),
            Authors = authors,
            MetadataTitle = source.MetadataTitle,
            MetadataDescription = source.MetadataDescription,
            MetadataKeywords = source.MetadataKeywords,
            PublishingDate = source.PublishingDate.Value ?? DateTime.MinValue
        };
    }
}
