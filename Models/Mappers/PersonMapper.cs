using Ficto.Generated.Models;
using Kontent.Ai.Delivery;

namespace Ficto.Models.Mappers;

public class PersonMapper : IAsyncMapper<Person, PersonViewModel>
{
    public async Task<PersonViewModel> MapAsync(Person source)
    {
        var bio = await source.Bio.ToHtmlAsync();

        return new PersonViewModel
        {
            Name = source.FirstName,
            LastName = source.LastName,
            Occupation = source.Occupation,
            Photograph = AssetViewModel.From(source.Photograph.FirstOrDefault()),
            Bio = bio
        };
    }
}
