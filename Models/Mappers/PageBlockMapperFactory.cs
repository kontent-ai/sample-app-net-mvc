using Ficto.Generated.Models;
using Ficto.Models.Helpers;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Delivery.ContentItems;

namespace Ficto.Models.Mappers;

public class PageBlockMapperFactory(
    ContentChunkMapper contentChunkMapper,
    VisualContainerMapper visualContainerMapper) : IPageBlockMapperFactory
{
    public async Task<PageBlockViewModel?> MapAsync(IEmbeddedContent content)
    {
        return content switch
        {
            // SDK wraps linked items as IEmbeddedContent<T>; raw T is fallback for edge cases.
            IEmbeddedContent<ContentChunk> embeddedChunk => await contentChunkMapper.MapAsync(embeddedChunk.Elements),
            IEmbeddedContent<VisualContainer> embeddedContainer => await visualContainerMapper.MapAsync(embeddedContainer.Elements),
            ContentChunk chunk => await contentChunkMapper.MapAsync(chunk),
            VisualContainer container => await visualContainerMapper.MapAsync(container),
            _ => null
        };
    }

    public async Task<IReadOnlyList<PageBlockViewModel>> MapManyAsync(IEnumerable<IEmbeddedContent> contents)
    {
        var mapped = await Task.WhenAll(contents.Select(MapAsync));
        return mapped.OfType<PageBlockViewModel>().ToList();
    }
}
