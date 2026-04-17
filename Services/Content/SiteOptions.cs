namespace Ficto.Services.Content;

/// <summary>
/// Static site-level options bound from configuration.
/// The active space (collection) for a given request comes from the cookie-stored
/// <see cref="Ficto.Configuration.ClientConfiguration"/> — not from here.
/// </summary>
public record SiteOptions
{
    /// <summary>
    /// Ordered list of collection/space codenames available in this Kontent.ai environment.
    /// The "default" collection is always included in queries but is not listed as a selectable space.
    /// </summary>
    public string[] Spaces { get; set; } = ["ficto_imaging", "ficto_healthtech", "ficto_surgical"];

    public int CacheExpirationSeconds { get; set; } = 60;

    public Dictionary<string, string> RouteTemplates { get; set; } = new()
    {
        ["page"] = "/{slug}",
        ["article"] = "/Articles/{slug}",
        ["product"] = "/Products/{slug}",
        ["solution"] = "/Solutions/{slug}",
    };
}