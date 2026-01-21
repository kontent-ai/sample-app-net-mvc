namespace Ficto.Models.Mappers;

public interface IAsyncMapper<in TSource, TDestination>
{
    Task<TDestination> MapAsync(TSource source);
}
