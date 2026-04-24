using Ficto.Generated.Models;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Mappers;

public class PersonMapper : IAsyncMapper<IContentItem<Person>, PersonViewModel>
{
    public Task<PersonViewModel> MapAsync(IContentItem<Person> source)
    {
        var e = source.Elements;

        return Task.FromResult(new PersonViewModel
        {
            ItemId = source.System.Id,
            Name = e.FirstName,
            LastName = e.LastName,
            Occupation = e.Occupation,
            Photograph = e.Photograph.FirstOrDefault(),
            Bio = e.Bio
        });
    }
}
