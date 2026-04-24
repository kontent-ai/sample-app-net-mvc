using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Ficto.Generated.Models;
using Ficto.Models.References;
using Ficto.Services.Routing;
using Kontent.Ai.Delivery;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Delivery.ContentItems.RichText.Resolution;
using ActionModel = Ficto.Generated.Models.Action;

namespace Ficto.Services.Content;

/// <summary>
/// Builds the single <see cref="IHtmlResolver"/> instance used by the <c>&lt;rich-text&gt;</c>
/// tag helper (resolved from DI). Registered as a singleton in Program.cs; the delegates below
/// are pure functions over their inputs plus the (singleton) <see cref="IRouteResolver"/>, so
/// there's no per-request state and no dependency on scoped services.
///
/// The three embedded-content resolvers (Fact, ActionModel, Callout) render inline HTML directly —
/// inline appearances are a distinct, simpler presentation than the block/card layouts used by
/// the <c>PageBlockMapperFactory</c> → partial pipeline elsewhere in the app.
/// </summary>
public static class RichTextResolver
{
    public static IHtmlResolver Build(IRouteResolver routes)
    {
        var builder = new HtmlResolverBuilder()
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
            });

        foreach (var tag in new[] { "h1", "h2", "h3", "h4", "h5", "h6" })
        {
            builder = builder.WithHtmlNodeResolver(tag, async (node, resolveChildren) =>
            {
                var innerHtml = await resolveChildren(node.Children);
                var id = Slugify(ExtractText(node.Children));
                var level = node.TagName.ToLowerInvariant();
                return $"<{level} class=\"rt-heading\"><a id=\"{Enc(id)}\" href=\"#{Enc(id)}\">{innerHtml}</a></{level}>";
            });
        }

        return builder
            .WithContentResolver<Fact>(content => new ValueTask<string>(RenderFact(content, routes)))
            .WithContentResolver<ActionModel>(content => new ValueTask<string>(RenderAction(content, routes)))
            .WithContentResolver<Callout>(async content => await RenderCalloutAsync(content))
            .Build();
    }

    // Emitted as a presence-only attribute on inline components so Kontent.ai Smart Link can
    // open them for editing in preview. Harmless in production (the SDK activates only via
    // query param or live-preview iframe), which keeps the resolver free of preview state.
    private static string KontentAttr(IContentItem item) =>
        $" data-kontent-component-id=\"{item.System.Id}\"";

    private static string RenderFact(IContentItem<Fact> source, IRouteResolver routes)
    {
        var fact = source.Elements;
        var reference = BuildReference(fact.ReferenceLabel, fact.ReferenceCaption, fact.ReferenceExternalUri, fact.ReferenceContentItemLink);
        var linkHtml = string.IsNullOrWhiteSpace(fact.ReferenceLabel)
            ? string.Empty
            : $"<a class=\"rt-fact__link\" href=\"{Enc(routes.ResolveUrl(reference))}\">{Enc(fact.ReferenceLabel)}</a>";

        return $"""
            <figure class="rt-fact"{KontentAttr(source)}>
                {(string.IsNullOrWhiteSpace(fact.Title) ? string.Empty : $"<figcaption class=\"rt-fact__title\">{Enc(fact.Title)}</figcaption>")}
                <blockquote class="rt-fact__message">{Enc(fact.Message)}</blockquote>
                {linkHtml}
            </figure>
            """;
    }

    private static string RenderAction(IContentItem<ActionModel> source, IRouteResolver routes)
    {
        var action = source.Elements;
        if (string.IsNullOrWhiteSpace(action.ReferenceLabel)) return string.Empty;

        var reference = BuildReference(action.ReferenceLabel, action.ReferenceCaption, action.ReferenceExternalUri, action.ReferenceContentItemLink);
        return $"""<a class="rt-action" href="{Enc(routes.ResolveUrl(reference))}"{KontentAttr(source)}>{Enc(action.ReferenceLabel)}</a>""";
    }

    private static async Task<string> RenderCalloutAsync(IContentItem<Callout> source)
    {
        var callout = source.Elements;
        var type = callout.Type.FirstOrDefault()?.Codename ?? "info";
        // Callout.Content is a RichTextContent element rendered with the SDK's default resolver
        // chain — callouts hold plain rich text, not further embedded items.
        var body = await callout.Content.ToHtmlAsync();
        return $"""<aside class="rt-callout rt-callout--{Enc(type)}"{KontentAttr(source)}>{body}</aside>""";
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

    private static string ExtractText(IReadOnlyList<IRichTextBlock> blocks)
    {
        var sb = new StringBuilder();
        Collect(blocks, sb);
        return sb.ToString();

        static void Collect(IReadOnlyList<IRichTextBlock> nodes, StringBuilder sb)
        {
            foreach (var n in nodes)
            {
                if (n is ITextNode t) sb.Append(t.Text);
                else if (n is IHtmlNode h) Collect(h.Children, sb);
            }
        }
    }

    private static string Slugify(string text)
    {
        var slug = Regex.Replace(text.ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');
        return slug.Length == 0 ? "heading" : slug;
    }
}
