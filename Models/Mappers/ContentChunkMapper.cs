using Ficto.Generated.Models;
using Kontent.Ai.Delivery.Extensions;

namespace Ficto.Models.Mappers;

public class ContentChunkMapper : IAsyncMapper<ContentChunk, ContentChunkViewModel>
{
    public async Task<ContentChunkViewModel> MapAsync(ContentChunk source)
    {
        var content = await source.Content.ToHtmlAsync();

        return new ContentChunkViewModel
        {
            Content = content
        };
    }
}
