namespace Ficto.Configuration;

/// <summary>
/// Configuration for preview-mode enable. The <see cref="Secret"/> value is compared
/// against the <c>?secret=</c> query parameter by <c>SpaceContextMiddleware</c>. Store it in
/// user-secrets or an environment variable for any reachable deployment — never commit a
/// real secret to source control.
/// </summary>
public class PreviewOptions
{
    /// <summary>
    /// Shared secret required in the <c>?secret=</c> query string to enable preview mode.
    /// Matched with a fixed-time comparison. If left empty the middleware logs a warning and
    /// admits any non-empty <c>?secret=</c> value, matching the sample's zero-config dev story.
    /// </summary>
    public string Secret { get; set; } = string.Empty;
}
