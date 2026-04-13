namespace Ficto.Services.Content;

/// <summary>
/// Provides the active collection/space codename for the current request.
/// Resolved once per request by <see cref="Ficto.Middleware.SpaceContextMiddleware"/>.
/// </summary>
public interface ISpaceContext
{
    string SpaceCodename { get; }
}
