using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models;

public record PersonViewModel
{
    public Guid? ItemId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Occupation { get; init; }
    public IAsset? Photograph { get; init; }
    public IRichTextContent? Bio { get; init; }
}