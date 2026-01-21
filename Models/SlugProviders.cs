using Ficto.Generated.Models;
using Ficto.Models.Helpers;

namespace Ficto.Generated.Models;

public partial record Article : ISlugProvider
{
    public string ContentType => "article";
}

public partial record Page : ISlugProvider
{
    public string ContentType => "page";
}

public partial record Product : ISlugProvider
{
    public string ContentType => "product";
}

public partial record Solution : ISlugProvider
{
    public string ContentType => "solution";
}
