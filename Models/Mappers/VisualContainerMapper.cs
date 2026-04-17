using Ficto.Generated.Models;
using Ficto.Models;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Mappers;

public class VisualContainerMapper(FactMapper factMapper) : IAsyncMapper<IContentItem<VisualContainer>, VisualContainerViewModel>
{
    public async Task<VisualContainerViewModel> MapAsync(IContentItem<VisualContainer> source)
    {
        var e = source.Elements;
        var items = await Task.WhenAll(
            e.Items.OfType<IContentItem<Fact>>().Select(factMapper.MapAsync));

        var visualRepresentation = e.VisualRepresentation.FirstOrDefault()?.Codename switch
        {
            "hero_unit" => VisualRepresentation.HeroUnit,
            "grid" => VisualRepresentation.Grid,
            _ => VisualRepresentation.Stack
        };

        return new VisualContainerViewModel
        {
            ItemId = source.System.Id,
            Title = e.Title,
            Subtitle = e.Subtitle,
            Items = items,
            VisualRepresentation = visualRepresentation
        };
    }
}
