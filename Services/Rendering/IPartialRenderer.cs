namespace Ficto.Services.Rendering;

public interface IPartialRenderer
{
    ValueTask<string> RenderAsync<TModel>(string partialName, TModel model, CancellationToken cancellationToken = default);
}
