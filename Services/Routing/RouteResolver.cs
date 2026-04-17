using Ficto.Models.References;
using Ficto.Services.Content;
using Microsoft.Extensions.Options;

namespace Ficto.Services.Routing;

public interface IRouteResolver
{
    string ResolveUrl(Reference? reference);
}

public class RouteResolver(IOptionsMonitor<SiteOptions> options) : IRouteResolver
{
    public string ResolveUrl(Reference? reference) => reference switch
    {
        ItemReference { Type: not null and var type, Slug: not null and var slug } =>
            options.CurrentValue.RouteTemplates.TryGetValue(type, out var template)
                ? template.Replace("{slug}", slug)
                : $"/{type}/{slug}",
        ItemReference => "#",
        UrlReference url => url.Url ?? "#",
        _ => "#"
    };
}
