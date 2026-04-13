using Ficto.Generated.Models;
using Ficto.Models;
using Ficto.Models.Helpers;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Delivery.ContentItems;

namespace Ficto.Models.Mappers;

public class VisualContainerMapper(FactMapper factMapper) : IAsyncMapper<VisualContainer, VisualContainerViewModel>
{
    public async Task<VisualContainerViewModel> MapAsync(VisualContainer source)
    {
        var items = new List<FactViewModel>();
        foreach (var embedded in source.Items.OfType<IEmbeddedContent<Fact>>())
        {
            items.Add(await factMapper.MapAsync(embedded.Elements));
        }

        var visualRepresentation = source.VisualRepresentation.FirstOrDefault()?.Codename switch
        {
            "hero_unit" => VisualRepresentation.HeroUnit,
            "grid" => VisualRepresentation.Grid,
            _ => VisualRepresentation.Stack
        };

        return new VisualContainerViewModel
        {
            Title = source.Title,
            Subtitle = source.Subtitle,
            Items = items,
            VisualRepresentation = visualRepresentation
        };
    }
}
