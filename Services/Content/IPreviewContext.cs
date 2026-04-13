namespace Ficto.Services.Content;

/// <summary>
/// Indicates whether the current request is serving preview (draft) content.
/// Resolved once per request by <see cref="Ficto.Middleware.SpaceContextMiddleware"/>
/// based on the presence of the <c>preview.</c> host prefix.
/// </summary>
public interface IPreviewContext
{
    bool IsPreview { get; }
}
