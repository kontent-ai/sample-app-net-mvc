using Ficto.Generated.Models;
using Ficto.Models.References;
using Ficto.Services.Routing;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Mappers;

public class FactMapper(ReferenceMapper referenceMapper, PersonMapper personMapper, IRouteResolver routeResolver)
    : IAsyncMapper<IContentItem<Fact>, FactViewModel>
{
    public async Task<FactViewModel> MapAsync(IContentItem<Fact> source)
    {
        var e = source.Elements;
        var reference = referenceMapper.Map(new ReferenceInput(
            e.ReferenceLabel,
            e.ReferenceCaption,
            e.ReferenceExternalUri,
            e.ReferenceContentItemLink
        ));

        var authors = await Task.WhenAll(
            e.Author.OfType<IContentItem<Person>>().Select(personMapper.MapAsync));

        return new FactViewModel
        {
            ItemId = source.System.Id,
            Title = e.Title,
            Message = e.Message,
            LinkLabel = string.IsNullOrWhiteSpace(e.ReferenceLabel) ? null : e.ReferenceLabel,
            LinkUrl = routeResolver.ResolveUrl(reference),
            LinkIsExternal = reference is UrlReference,
            Image = AssetViewModel.From(e.Image.FirstOrDefault()),
            Authors = authors
        };
    }
}
