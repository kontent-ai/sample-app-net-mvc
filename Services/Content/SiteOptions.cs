namespace Ficto.Services.Content;

/// <summary>
/// Contextual information about current site (collection). May also include selected language in the future.
/// </summary>
public record SiteOptions
{
    public string CollectionCodename { get; set; } = "ficto_healthtech";
    public int CacheExpirationSeconds { get; set; } = 60;
}