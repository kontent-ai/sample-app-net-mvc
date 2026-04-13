namespace Ficto.Services.Content;

/// <summary>
/// Contextual information about current site (collection). May also include selected language in the future.
/// </summary>
public record SiteOptions
{
    public string CollectionCodename { get; set; } = "ficto_healthtech";
    public int CacheExpirationSeconds { get; set; } = 60;

    public Dictionary<string, string> RouteTemplates { get; set; } = new()
    {
        ["page"] = "/{slug}",
        ["article"] = "/Articles/{slug}",
        ["product"] = "/Products/{slug}",
        ["solution"] = "/Solutions/{slug}",
    };
}