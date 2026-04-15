namespace Ficto.Services.Content;

/// <summary>
/// Issues and validates the signed cookie payload that marks a request as preview-enabled.
/// Wraps <see cref="Microsoft.AspNetCore.DataProtection.IDataProtectionProvider"/> so callers
/// don't have to think about purposes, keys, or ciphertext formats.
/// </summary>
public interface IPreviewTokenProtector
{
    string Issue();
    bool IsValid(string? tokenFromCookie);
}
