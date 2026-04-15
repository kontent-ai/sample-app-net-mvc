using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace Ficto.Services.Content;

public sealed class PreviewTokenProtector(IDataProtectionProvider provider) : IPreviewTokenProtector
{
    // Bumping the purpose string invalidates every existing preview cookie on next deploy.
    private const string Purpose = "Ficto.Preview.v1";
    private const string Payload = "enabled";

    private readonly IDataProtector _protector = provider.CreateProtector(Purpose);

    public string Issue() => _protector.Protect(Payload);

    public bool IsValid(string? tokenFromCookie)
    {
        if (string.IsNullOrEmpty(tokenFromCookie)) return false;

        try
        {
            return _protector.Unprotect(tokenFromCookie) == Payload;
        }
        catch (CryptographicException)
        {
            return false;
        }
    }
}
