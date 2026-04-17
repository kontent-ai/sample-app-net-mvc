namespace Ficto.Configuration;

/// <summary>
/// Configuration for the <see cref="Ficto.Services.Content.SecretPreviewAccessGate"/>.
/// Store <c>Secret</c> in user-secrets or an environment variable — never commit it to source control.
/// </summary>
public class PreviewOptions
{
    /// <summary>
    /// Shared secret required in the <c>?secret=</c> query string of <c>/preview/enable</c>.
    /// If left empty, the secret gate falls back to anonymous access and logs a warning.
    /// </summary>
    public string Secret { get; set; } = string.Empty;
}
