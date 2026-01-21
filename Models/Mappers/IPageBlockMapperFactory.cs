using Ficto.Models.Helpers;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Mappers;

public interface IPageBlockMapperFactory
{
    Task<PageBlockViewModel?> MapAsync(IEmbeddedContent content);
    Task<IReadOnlyList<PageBlockViewModel>> MapManyAsync(IEnumerable<IEmbeddedContent> contents);
}
