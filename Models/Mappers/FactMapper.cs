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

        var authors = await Task.WhenAll(
            source.Author.OfType<IContentItem<Person>>().Select(a => personMapper.MapAsync(a.Elements)));

        return new FactViewModel
        {
            Title = source.Title,
            Message = source.Message,
            LinkLabel = string.IsNullOrWhiteSpace(source.ReferenceLabel) ? null : source.ReferenceLabel,
            LinkUrl = routeResolver.ResolveUrl(reference),
            LinkIsExternal = reference is UrlReference,
            Image = AssetViewModel.From(source.Image.FirstOrDefault()),
            Authors = authors
        };
    }
}
