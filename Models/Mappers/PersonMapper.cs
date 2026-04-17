using Ficto.Generated.Models;
using Kontent.Ai.Delivery;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Mappers;

public class PersonMapper(IHtmlResolver htmlResolver) : IAsyncMapper<IContentItem<Person>, PersonViewModel>
{
    public async Task<PersonViewModel> MapAsync(IContentItem<Person> source)
    {
        var e = source.Elements;
        var bio = await e.Bio.ToHtmlAsync(htmlResolver);

        return new PersonViewModel
        {
            ItemId = source.System.Id,
            Name = e.FirstName,
            LastName = e.LastName,
            Occupation = e.Occupation,
            Photograph = AssetViewModel.From(e.Photograph.FirstOrDefault()),
            Bio = bio
        };
    }
}
