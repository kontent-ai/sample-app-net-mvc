using System.Text.Encodings.Web;
using Ficto.Generated.Models;
using Ficto.Models.Helpers;
using Ficto.Models.Mappers;
using Ficto.Services.Rendering;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Delivery.ContentItems.RichText.Resolution;

namespace Ficto.Services.Content.RichText;

public static class RichTextResolverServiceCollectionExtensions
{
    public static IServiceCollection AddFictoRichTextResolver(this IServiceCollection services)
    {
        services.AddScoped<IHtmlResolver>(sp =>
        {
            var routes = sp.GetRequiredService<IRouteResolver>();
            var blockFactory = sp.GetRequiredService<IPageBlockMapperFactory>();
            var partialRenderer = sp.GetRequiredService<IPartialRenderer>();
            var logger = sp.GetRequiredService<ILogger<RichTextResolverMarker>>();

            return new HtmlResolverBuilder()
                .WithContentItemLinkResolver(async (link, resolveChildren) =>
                {
                    var innerHtml = await resolveChildren(link.Children);

                    var url = link.Metadata is { } metadata
                        ? routes.ResolveUrl(new ItemReference
                        {
                            Type = metadata.ContentTypeCodename,
                            Slug = string.IsNullOrEmpty(metadata.UrlSlug) ? metadata.Codename : metadata.UrlSlug
                        })
                        : "#";

                    var attributes = string.Concat(link.Attributes
                        .Where(kv => !string.Equals(kv.Key, "href", StringComparison.OrdinalIgnoreCase))
                        .Select(kv => $" {kv.Key}=\"{HtmlEncoder.Default.Encode(kv.Value)}\""));

                    return $"<a href=\"{HtmlEncoder.Default.Encode(url)}\" data-item-id=\"{link.ItemId}\"{attributes}>{innerHtml}</a>";
                })
                .WithContentResolver<ContentChunk>(content => RenderEmbeddedAsync(content, blockFactory, partialRenderer, logger))
                .WithContentResolver<VisualContainer>(content => RenderEmbeddedAsync(content, blockFactory, partialRenderer, logger))
                .Build();
        });

        return services;
    }

    private static async ValueTask<string> RenderEmbeddedAsync<T>(
        IEmbeddedContent<T> content,
        IPageBlockMapperFactory blockFactory,
        IPartialRenderer partialRenderer,
        ILogger logger)
    {
        var viewModel = await blockFactory.MapAsync(content);
        if (viewModel is null)
        {
            logger.LogWarning(
                "No page-block mapper for inline embedded content of type {Type} (codename {Codename}).",
                typeof(T).Name, content.System?.Codename);
            return string.Empty;
        }

        return await partialRenderer.RenderAsync(viewModel.PartialViewName, viewModel);
    }

    internal sealed class RichTextResolverMarker;
}
