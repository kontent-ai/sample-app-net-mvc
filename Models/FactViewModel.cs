using Kontent.Ai.Delivery.Abstractions;
using Ficto.Models.Helpers;

namespace Ficto.Models;

public record FactViewModel
{
    public string? Title { get; init; }
    public string Message { get; init; } = string.Empty;
    public Reference? Reference { get; init; }
    public IAsset? Image { get; init; }
}