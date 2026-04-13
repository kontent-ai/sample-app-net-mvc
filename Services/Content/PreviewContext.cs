namespace Ficto.Services.Content;

/// <summary>
/// Mutable, scoped implementation of <see cref="IPreviewContext"/>.
/// Written by <see cref="Ficto.Middleware.SpaceContextMiddleware"/>; read by downstream services and views.
/// </summary>
public class PreviewContext : IPreviewContext
{
    public bool IsPreview { get; set; }
}
