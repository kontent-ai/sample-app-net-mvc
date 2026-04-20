using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Services.Content;

public partial interface IContentService
{
    /// <summary>
    /// Fetches the <c>product_category</c> taxonomy group, including its hierarchical terms.
    /// </summary>
    /// <returns>The taxonomy group, or null if not found.</returns>
    /// <exception cref="ContentDeliveryException">Thrown when the Delivery API returns a server error (5xx).</exception>
    Task<ITaxonomyGroup?> GetProductCategoryTaxonomyAsync(CancellationToken ct = default);
}
