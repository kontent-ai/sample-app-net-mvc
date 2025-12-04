using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Services.Content.Interfaces;

public partial interface IContentService
{
    Task<IDeliveryResult<IContentItem<T>>> GetItemAsync<T>(string codename) where T : class, IElementsModel;
}
