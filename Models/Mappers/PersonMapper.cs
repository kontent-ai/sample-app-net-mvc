using Ficto.Generated.Models;
using Kontent.Ai.Delivery;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Mappers;

public class PersonMapper(IHtmlResolver htmlResolver) : IAsyncMapper<Person, PersonViewModel>
{
    public async Task<PersonViewModel> MapAsync(Person source)
    {
        var bio = await source.Bio.ToHtmlAsync(htmlResolver);

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
