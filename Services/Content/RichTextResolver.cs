using System.Text.Encodings.Web;
using Ficto.Generated.Models;
using Ficto.Models.Helpers;
using Kontent.Ai.Delivery;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Delivery.ContentItems.RichText.Resolution;
using ActionModel = Ficto.Generated.Models.Action;

namespace Ficto.Services.Content;

/// <summary>
/// Builds the single <see cref="IHtmlResolver"/> instance used by every rich-text <c>.ToHtmlAsync</c>
/// call in the app. Registered as a singleton in Program.cs; the delegates below are pure functions
/// over their inputs plus the (singleton) <see cref="IRouteResolver"/>, so there's no per-request
/// state and no dependency on scoped services.
///
/// The three embedded-content resolvers (Fact, ActionModel, Callout) render inline HTML directly —
/// inline appearances are a distinct, simpler presentation than the block/card layouts used by
/// the <c>PageBlockMapperFactory</c> → partial pipeline elsewhere in the app. Keeping them as
/// inline templates avoids pulling the mapper graph into rich-text resolution (which created a
/// ContentChunkMapper ↔ IHtmlResolver cycle when it was attempted earlier).
/// </summary>
public static class RichTextResolver
{
    public static IHtmlResolver Build(IRouteResolver routes) =>
        new HtmlResolverBuilder()
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
                    .Select(kv => $" {kv.Key}=\"{Enc(kv.Value)}\""));

                return $"<a href=\"{Enc(url)}\" data-item-id=\"{link.ItemId}\"{attributes}>{innerHtml}</a>";
            })
            .WithContentResolver<Fact>(content => new ValueTask<string>(RenderFact(content.Elements, routes)))
            .WithContentResolver<ActionModel>(content => new ValueTask<string>(RenderAction(content.Elements, routes)))
            .WithContentResolver<Callout>(async content => await RenderCalloutAsync(content.Elements))
            .Build();

    private static string RenderFact(Fact fact, IRouteResolver routes)
    {
        var reference = BuildReference(fact.ReferenceLabel, fact.ReferenceCaption, fact.ReferenceExternalUri, fact.ReferenceContentItemLink);
        var linkHtml = reference is null
            ? string.Empty
            : $"<a class=\"rt-fact__link\" href=\"{Enc(routes.ResolveUrl(reference))}\">{Enc(reference.Label ?? string.Empty)}</a>";

        return $"""
            <figure class="rt-fact">
                {(string.IsNullOrWhiteSpace(fact.Title) ? string.Empty : $"<figcaption class=\"rt-fact__title\">{Enc(fact.Title)}</figcaption>")}
                <blockquote class="rt-fact__message">{Enc(fact.Message)}</blockquote>
                {linkHtml}
            </figure>
            """;
    }

    private static string RenderAction(ActionModel action, IRouteResolver routes)
    {
        var reference = BuildReference(action.ReferenceLabel, action.ReferenceCaption, action.ReferenceExternalUri, action.ReferenceContentItemLink);
        if (reference is null) return string.Empty;

        var label = reference.Label ?? string.Empty;
        return $"""<a class="rt-action" href="{Enc(routes.ResolveUrl(reference))}">{Enc(label)}</a>""";
    }

    private static async Task<string> RenderCalloutAsync(Callout callout)
    {
        var type = callout.Type.FirstOrDefault()?.Codename ?? "info";
        // Callout.Content is a RichTextContent element rendered with the SDK's default resolver
        // chain — callouts hold plain rich text, not further embedded items.
        var body = await callout.Content.ToHtmlAsync();
        return $"""<aside class="rt-callout rt-callout--{Enc(type)}">{body}</aside>""";
    }

    private static Reference? BuildReference(
        string? label,
        string? caption,
        string? externalUri,
        IEnumerable<IEmbeddedContent>? contentItemLink)
    {
        if (!string.IsNullOrWhiteSpace(externalUri))
            return new UrlReference { Label = label, Caption = caption, Url = externalUri };

        var linked = contentItemLink?
            .OfType<IContentItem>()
            .Select(ci => ci.System)
            .FirstOrDefault();

        if (linked is not null)
            return new ItemReference { Label = label, Caption = caption, Type = linked.Type, Slug = linked.Codename };

        return null;
    }

    private static string Enc(string value) => HtmlEncoder.Default.Encode(value);
}
