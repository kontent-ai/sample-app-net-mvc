using Ficto.Generated.Models;
using Ficto.Models.Helpers;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Delivery.ContentItems;

namespace Ficto.Models.Mappers;

public class VisualContainerMapper(FactMapper factMapper) : IAsyncMapper<VisualContainer, VisualContainerViewModel>
{
    private readonly FactMapper _factMapper = factMapper;

    public Task<VisualContainerViewModel> MapAsync(VisualContainer source)
    {
        var items = source.Items
            .OfType<IEmbeddedContent<Fact>>()
            .Select(embedded => _factMapper.Map(embedded.Elements))
            .ToList();

        var visualRepresentation = ParseVisualRepresentation(source.VisualRepresentation.FirstOrDefault()?.Codename);

        var result = new VisualContainerViewModel
        {
            Title = source.Title,
            Subtitle = source.Subtitle,
            Items = items,
            VisualRepresentation = visualRepresentation
        };

        return Task.FromResult(result);
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
