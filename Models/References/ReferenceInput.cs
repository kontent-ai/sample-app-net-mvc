using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.References;

public record ReferenceInput(
    string? Label,
    string? Caption,
    string? ExternalUri,
    IEnumerable<IEmbeddedContent>? ContentItemLink
);
