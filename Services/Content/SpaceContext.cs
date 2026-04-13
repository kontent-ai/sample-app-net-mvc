namespace Ficto.Services.Content;

/// <summary>
/// Mutable, scoped implementation of <see cref="ISpaceContext"/>.
/// The middleware sets <see cref="SpaceCodename"/> early in the pipeline;
/// downstream services consume it via <see cref="ISpaceContext"/>.
/// </summary>
public class SpaceContext : ISpaceContext
{
    public string SpaceCodename { get; set; } = string.Empty;
}
