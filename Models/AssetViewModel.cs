using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Urls.ImageTransformation;

namespace Ficto.Models;

/// <summary>
/// View-friendly projection of <see cref="IAsset"/> exposing only the fields views need,
/// plus a helper that builds transformed image URLs via the Delivery API's image transformations.
/// </summary>
public record AssetViewModel
{
    public string Url { get; init; } = string.Empty;

    /// <summary>Alt text for <c>&lt;img&gt;</c>, sourced from the asset's Description field.</summary>
    public string? AltText { get; init; }

    public int? Width { get; init; }
    public int? Height { get; init; }

    public static AssetViewModel? From(IAsset? asset) =>
        asset is null
            ? null
            : new AssetViewModel
            {
                Url = asset.Url,
                AltText = asset.Description,
                Width = asset.Width,
                Height = asset.Height,
            };

    /// <summary>
    /// Builds a transformed asset URL using Kontent.ai's Delivery image transformation API.
    /// Demonstrates responsive image delivery without re-uploading assets in different sizes.
    /// </summary>
    public string UrlWithTransform(
        int? width = null,
        int? height = null,
        ImageFormat? format = null,
        int? quality = null)
    {
        var builder = new ImageUrlBuilder(Url);
        if (width.HasValue) builder.WithWidth(width.Value);
        if (height.HasValue) builder.WithHeight(height.Value);
        if (format.HasValue) builder.WithFormat(format.Value);
        if (quality.HasValue) builder.WithQuality(quality.Value);
        return builder.Url.ToString();
    }
}
