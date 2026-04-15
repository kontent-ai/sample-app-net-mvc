namespace Ficto.Services.Content;

/// <summary>
/// Sample-only <see cref="IPreviewAccessGate"/> that allows every enable request. Emits a warning
/// each time so accidental production use is loud. Replace with an auth-aware implementation
/// before shipping anything real.
/// </summary>
public sealed class AllowAnonymousPreviewAccessGate(ILogger<AllowAnonymousPreviewAccessGate> logger)
    : IPreviewAccessGate
{
    public ValueTask<bool> CanEnableAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        logger.LogWarning(
            "Preview enable request allowed without authentication — the default {Gate} is in use. " +
            "Register a real IPreviewAccessGate implementation for non-sample deployments.",
            nameof(AllowAnonymousPreviewAccessGate));
        return ValueTask.FromResult(true);
    }
}
