using Ficto.Generated.Models;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Mappers;

public class ContentChunkMapper : IAsyncMapper<IContentItem<ContentChunk>, ContentChunkViewModel>
{
    public Task<ContentChunkViewModel> MapAsync(IContentItem<ContentChunk> source) =>
        Task.FromResult(new ContentChunkViewModel
        {
            ItemId = source.System.Id,
            Content = source.Elements.Content
        });
}
