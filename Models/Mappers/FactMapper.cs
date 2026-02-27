using Ficto.Generated.Models;
using Ficto.Models.Helpers;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Mappers;

public class FactMapper(ReferenceMapper referenceMapper, PersonMapper personMapper) : IAsyncMapper<Fact, FactViewModel>
{
    private readonly ReferenceMapper _referenceMapper = referenceMapper;
    private readonly PersonMapper _personMapper = personMapper;

    public async Task<FactViewModel> MapAsync(Fact source)
    {
        var reference = _referenceMapper.Map(new ReferenceInput(
            source.ReferenceLabel,
            source.ReferenceCaption,
            source.ReferenceExternalUri,
            source.ReferenceContentItemLink
        ));

        var authors = new List<PersonViewModel>();
        foreach (var author in source.Author.OfType<IContentItem<Person>>())
        {
            authors.Add(await _personMapper.MapAsync(author.Elements));
        }

        return new FactViewModel
        {
            Title = source.Title,
            Message = source.Message,
            LinkLabel = reference?.Label,
            LinkUrl = RouteHelper.ResolveUrl(reference),
            LinkIsExternal = reference is UrlReference,
            Image = source.Image.FirstOrDefault(),
            Authors = authors
        };
    }
}
