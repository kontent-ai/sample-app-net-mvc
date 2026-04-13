using Ficto.Generated.Models;
using Ficto.Models.Helpers;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Mappers;

public class FactMapper(ReferenceMapper referenceMapper, PersonMapper personMapper, IRouteResolver routeResolver)
    : IAsyncMapper<Fact, FactViewModel>
{
    public async Task<FactViewModel> MapAsync(Fact source)
    {
        var reference = referenceMapper.Map(new ReferenceInput(
            source.ReferenceLabel,
            source.ReferenceCaption,
            source.ReferenceExternalUri,
            source.ReferenceContentItemLink
        ));

        var authors = new List<PersonViewModel>();
        foreach (var author in source.Author.OfType<IContentItem<Person>>())
        {
            authors.Add(await personMapper.MapAsync(author.Elements));
        }

        return new FactViewModel
        {
            Title = source.Title,
            Message = source.Message,
            LinkLabel = reference?.Label,
            LinkUrl = routeResolver.ResolveUrl(reference),
            LinkIsExternal = reference is UrlReference,
            Image = AssetViewModel.From(source.Image.FirstOrDefault()),
            Authors = authors
        };
    }
}
