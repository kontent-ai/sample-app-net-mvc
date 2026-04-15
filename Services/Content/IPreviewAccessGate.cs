namespace Ficto.Services.Content;

/// <summary>
/// Decides whether the current request may toggle preview mode on. The default
/// <see cref="AllowAnonymousPreviewAccessGate"/> registered in <c>Program.cs</c> allows every
/// request and is intended only for sample/demo use. In production, register your own
/// implementation that verifies the caller against your auth system (OIDC / OAuth / custom SSO
/// / an ASP.NET Core authorization policy).
/// </summary>
public interface IPreviewAccessGate
{
    ValueTask<bool> CanEnableAsync(HttpContext context, CancellationToken cancellationToken = default);
}
