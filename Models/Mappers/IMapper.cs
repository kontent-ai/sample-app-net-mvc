namespace Ficto.Models.Mappers;

public interface IMapper<TSource, TDestination>
{
    TDestination Map(TSource source);
}