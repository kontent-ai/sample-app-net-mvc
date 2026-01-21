using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Helpers;

public record ReferenceInput(
    string? Label,
    string? Caption,
    string? ExternalUri,
    IEnumerable<IEmbeddedContent>? ContentItemLink
);
