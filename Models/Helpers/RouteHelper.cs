namespace Ficto.Models.Helpers;

public static class RouteHelper
{
    public static string ResolveUrl(Reference? reference) => reference switch
    {
        ItemReference itemRef => itemRef.Type switch
        {
            "product" => $"/Products/{itemRef.Slug}",
            _ => $"/{itemRef.Type}/{itemRef.Slug}"
        },
        UrlReference urlRef => urlRef.Url ?? "#",
        _ => "#"
    };
}
