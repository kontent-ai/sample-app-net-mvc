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

    /// <summary>
    /// Projects an <see cref="IAsset"/> into a view-friendly shape. When
    /// <paramref name="preferredRendition"/> is supplied and the asset carries a matching preset,
    /// the rendition's transformation query is applied on top of the base URL; missing presets
    /// fall back transparently to the untransformed URL.
    /// </summary>
    public static AssetViewModel? From(IAsset? asset, string? preferredRendition = null)
    {
        if (asset is null) return null;

        var url = asset.Url;
        var width = asset.Width;
        var height = asset.Height;

        if (preferredRendition is not null
            && asset.Renditions.TryGetValue(preferredRendition, out var rendition))
        {
            if (!string.IsNullOrEmpty(rendition.Query))
                url = $"{url}?{rendition.Query}";
            width = rendition.Width;
            height = rendition.Height;
        }

        return new AssetViewModel
        {
            Url = url,
            AltText = asset.Description,
            Width = width,
            Height = height,
        };
    }

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
