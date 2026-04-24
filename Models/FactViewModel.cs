using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models;

public record FactViewModel
{
    public Guid? ItemId { get; init; }
    public string? Title { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? LinkLabel { get; init; }
    public string? LinkUrl { get; init; }
    public bool LinkIsExternal { get; init; }
    public IAsset? Image { get; init; }
    public IReadOnlyList<PersonViewModel> Authors { get; init; } = [];
}