namespace Ficto.Models.References;

public interface ISlugProvider
{
    string Slug { get; }
    string ContentType { get; }
}
