namespace Ficto.Models;

public record ContentChunkViewModel : PageBlockViewModel
{
    public Guid? ItemId { get; init; }
    public string Content { get; init; } = string.Empty;
    public override string PartialViewName => "_ContentChunk";
}