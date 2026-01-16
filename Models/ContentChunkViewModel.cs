using Ficto.Models.Helpers;

namespace Ficto.Models;

public record ContentChunkViewModel : PageBlockViewModel
{
    public string Content { get; init; } = string.Empty;
    public override string PartialViewName => "_ContentChunk";
}