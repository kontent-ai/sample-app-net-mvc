using Ficto.Generated.Models;
using Ficto.Models.Helpers;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Mappers;

public class PageBlockMapperFactory(
    ContentChunkMapper contentChunkMapper,
    VisualContainerMapper visualContainerMapper) : IPageBlockMapperFactory
{
    public async Task<PageBlockViewModel?> MapAsync(IEmbeddedContent content)
    {
        // IEmbeddedContent<T> : IContentItem<T>, so mappers can take the full wrapper
        // directly and read both System metadata (for Smart Link) and Elements.
        return content switch
        {
            IContentItem<ContentChunk> chunk => await contentChunkMapper.MapAsync(chunk),
            IContentItem<VisualContainer> container => await visualContainerMapper.MapAsync(container),
            _ => null
        };
    }

    public async Task<IReadOnlyList<PageBlockViewModel>> MapManyAsync(IEnumerable<IEmbeddedContent> contents)
    {
        var mapped = await Task.WhenAll(contents.Select(MapAsync));
        return mapped.OfType<PageBlockViewModel>().ToList();
    }
}
