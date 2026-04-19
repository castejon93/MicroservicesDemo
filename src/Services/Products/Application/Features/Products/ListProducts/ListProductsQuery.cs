using Products.Application.Cqrs;
using Products.Application.DTOs;

namespace Products.Application.Features.Products.ListProducts
{
    public sealed record ListProductsQuery : IQuery<IReadOnlyList<ProductDto>>;
}