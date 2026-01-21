using Ficto.Generated.Models;
using Ficto.Models.Helpers;

namespace Ficto.Models.Mappers;

public class FactMapper(ReferenceMapper referenceMapper) : IMapper<Fact, FactViewModel>
{
    private readonly ReferenceMapper _referenceMapper = referenceMapper;

    public FactViewModel Map(Fact source)
    {
        var reference = _referenceMapper.Map(new ReferenceInput(
            source.ReferenceLabel,
            source.ReferenceCaption,
            source.ReferenceExternalUri,
            source.ReferenceContentItemLink
        ));

        return new FactViewModel
        {
            Title = source.Title,
            Message = source.Message,
            Reference = reference,
            Image = source.Image.FirstOrDefault()
        };
    }
}
