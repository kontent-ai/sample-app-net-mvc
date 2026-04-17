using System.Security.Cryptography;
using System.Text;
using Ficto.Configuration;
using Microsoft.Extensions.Options;

namespace Ficto.Services.Content;

/// <summary>
/// Default preview gate for the sample. Checks a shared secret supplied in the
/// <c>?secret=</c> query string against <see cref="PreviewOptions.Secret"/> using a fixed-time
/// comparison to avoid leaking length/timing information.
/// <para>
/// If <see cref="PreviewOptions.Secret"/> is empty, the gate allows any request and logs a warning,
/// matching the zero-config local-dev behaviour of <see cref="AllowAnonymousPreviewAccessGate"/>.
/// </para>
/// </summary>
public sealed class SecretPreviewAccessGate(
    IOptions<PreviewOptions> options,
    ILogger<SecretPreviewAccessGate> logger) : IPreviewAccessGate
{
    private readonly PreviewOptions _options = options.Value;

    public ValueTask<bool> CanEnableAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_options.Secret))
        {
            logger.LogWarning(
                "Preview enable allowed without a secret — PreviewOptions:Secret is not configured. " +
                "Set it in user-secrets (dotnet user-secrets set PreviewOptions:Secret <value>) for real deployments.");
            return ValueTask.FromResult(true);
        }

        var supplied = context.Request.Query["secret"].ToString();
        if (string.IsNullOrEmpty(supplied)) return ValueTask.FromResult(false);

        var expectedBytes = Encoding.UTF8.GetBytes(_options.Secret);
        var suppliedBytes = Encoding.UTF8.GetBytes(supplied);
        var match = CryptographicOperations.FixedTimeEquals(expectedBytes, suppliedBytes);
        return ValueTask.FromResult(match);
    }
}
