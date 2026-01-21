namespace Ficto.Models.Helpers;

public interface ISlugProvider
{
    string Slug { get; }
    string ContentType { get; }
}
