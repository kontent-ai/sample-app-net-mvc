using Ficto.Generated.Models;
using Ficto.Models.Helpers;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Delivery.ContentItems;

namespace Ficto.Models.Mappers;

public class VisualContainerMapper(FactMapper factMapper) : IAsyncMapper<VisualContainer, VisualContainerViewModel>
{
    private readonly FactMapper _factMapper = factMapper;

    public async Task<VisualContainerViewModel> MapAsync(VisualContainer source)
    {
        var items = new List<FactViewModel>();
        foreach (var embedded in source.Items.OfType<IEmbeddedContent<Fact>>())
        {
            items.Add(await _factMapper.MapAsync(embedded.Elements));
        }

        var visualRepresentation = ParseVisualRepresentation(source.VisualRepresentation.FirstOrDefault()?.Codename);

        return new VisualContainerViewModel
        {
            Title = source.Title,
            Subtitle = source.Subtitle,
            Items = items,
            VisualRepresentation = visualRepresentation
        };
    }

    private static VisualRepresentation ParseVisualRepresentation(string? codename)
    {
        return codename switch
        {
            "hero_unit" => VisualRepresentation.HeroUnit,
            "grid" => VisualRepresentation.Grid,
            "stack" => VisualRepresentation.Stack,
            _ => VisualRepresentation.Stack
        };
    }
}
