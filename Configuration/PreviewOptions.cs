namespace Ficto.Configuration;

/// <summary>
/// Options for preview mode. Bind from configuration; store the secret in user-secrets or environment variables.
/// </summary>
public class PreviewOptions
{
    /// <summary>
    /// Shared secret used to verify the <c>/preview/enable</c> request.
    /// Must match the value set in Kontent.ai preview URL configuration.
    /// </summary>
    public string PreviewAccessSecret { get; set; } = string.Empty;
}
