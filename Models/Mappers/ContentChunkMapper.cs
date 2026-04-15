using Ficto.Generated.Models;
using Kontent.Ai.Delivery;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Mappers;

public class ContentChunkMapper(IHtmlResolver htmlResolver) : IAsyncMapper<ContentChunk, ContentChunkViewModel>
{
    public async Task<ContentChunkViewModel> MapAsync(ContentChunk source)
    {
        var content = await source.Content.ToHtmlAsync(htmlResolver);

        return new ContentChunkViewModel
        {
            Content = content
        };
    }
}
