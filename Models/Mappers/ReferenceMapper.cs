using Ficto.Models.References;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Models.Mappers;

public class ReferenceMapper : IMapper<ReferenceInput, Reference?>
{
    public Reference? Map(ReferenceInput source)
    {
        // Item link takes priority over external URL
        // SDK wraps linked items as IContentItem<T>, so unwrap via .Elements
        var linkedItem = source.ContentItemLink?.FirstOrDefault();
        var inner = linkedItem is IContentItem contentItem ? contentItem.Elements : linkedItem;
        if (inner is ISlugProvider slugProvider)
        {
            return new ItemReference
            {
                Label = source.Label,
                Caption = source.Caption,
                Slug = slugProvider.Slug,
                Type = slugProvider.ContentType
            };
        }

        // Fall back to external URL
        if (!string.IsNullOrWhiteSpace(source.ExternalUri))
        {
            return new UrlReference
            {
                Label = source.Label,
                Caption = source.Caption,
                Url = source.ExternalUri
            };
        }

        return null;
    }
}
