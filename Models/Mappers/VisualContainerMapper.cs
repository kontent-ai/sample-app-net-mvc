using Ficto.Generated.Models;
using Ficto.Models.Helpers;

namespace Ficto.Models.Mappers;

public class VisualContainerMapper(FactMapper factMapper) : IAsyncMapper<VisualContainer, VisualContainerViewModel>
{
    private readonly FactMapper _factMapper = factMapper;

    public Task<VisualContainerViewModel> MapAsync(VisualContainer source)
    {
        var items = source.Items
            .OfType<Fact>()
            .Select(_factMapper.Map)
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
