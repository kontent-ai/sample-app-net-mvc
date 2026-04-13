using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models;

public record AssetViewModel
{
    public string Url { get; init; } = string.Empty;
    public string? Description { get; init; }

    public static AssetViewModel? From(IAsset? asset) =>
        asset is null ? null : new AssetViewModel { Url = asset.Url, Description = asset.Description };
}
