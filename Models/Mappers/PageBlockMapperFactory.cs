using Ficto.Generated.Models;
using Ficto.Models.Helpers;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Mappers;

public class PageBlockMapperFactory(
    ContentChunkMapper contentChunkMapper,
    VisualContainerMapper visualContainerMapper) : IPageBlockMapperFactory
{
    private readonly ContentChunkMapper _contentChunkMapper = contentChunkMapper;
    private readonly VisualContainerMapper _visualContainerMapper = visualContainerMapper;

    public async Task<PageBlockViewModel?> MapAsync(IEmbeddedContent content)
    {
        return content switch
        {
            ContentChunk chunk => await _contentChunkMapper.MapAsync(chunk),
            VisualContainer container => await _visualContainerMapper.MapAsync(container),
            _ => null
        };
    }

    public async Task<IReadOnlyList<PageBlockViewModel>> MapManyAsync(IEnumerable<IEmbeddedContent> contents)
    {
        var results = new List<PageBlockViewModel>();

        foreach (var content in contents)
        {
            var mapped = await MapAsync(content);
            if (mapped != null)
            {
                results.Add(mapped);
            }
        }

        return results;
    }
}
