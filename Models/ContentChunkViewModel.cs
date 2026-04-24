using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models;

public record ContentChunkViewModel : PageBlockViewModel
{
    public Guid? ItemId { get; init; }
    public IRichTextContent? Content { get; init; }
    public override string PartialViewName => "_ContentChunk";
}