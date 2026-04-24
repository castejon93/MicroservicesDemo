using Products.Application.Cqrs;
using Products.Application.DTOs;

namespace Products.Application.Features.Products.ListProducts
{
    /// <summary>
    /// Read-only query that returns every product in the catalog as a list of
    /// <see cref="ProductDto"/>. No paging or filtering is applied yet; callers
    /// should be aware the whole table is materialized.
    /// </summary>
    public sealed record ListProductsQuery : IQuery<IReadOnlyList<ProductDto>>;
}