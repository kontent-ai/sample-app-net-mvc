namespace Ficto.Configuration;

/// <summary>
/// Options for the Kontent.ai webhook endpoint. Bind from configuration; store the secret in user-secrets.
/// </summary>
public class WebhookOptions
{
    /// <summary>
    /// HMAC-SHA256 secret used to verify incoming webhook requests via the <c>X-KC-Signature</c> header.
    /// </summary>
    public string Secret { get; set; } = string.Empty;
}
