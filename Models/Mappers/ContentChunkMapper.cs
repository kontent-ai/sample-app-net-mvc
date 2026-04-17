using Ficto.Generated.Models;
using Kontent.Ai.Delivery;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Mappers;

public class ContentChunkMapper(IHtmlResolver htmlResolver) : IAsyncMapper<IContentItem<ContentChunk>, ContentChunkViewModel>
{
    public async Task<ContentChunkViewModel> MapAsync(IContentItem<ContentChunk> source)
    {
        var content = await source.Elements.Content.ToHtmlAsync(htmlResolver);

        return new ContentChunkViewModel
        {
            ItemId = source.System.Id,
            Content = content
        };
    }
}
