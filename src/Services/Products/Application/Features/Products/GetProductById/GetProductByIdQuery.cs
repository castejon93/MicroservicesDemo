using Products.Application.Cqrs;
using Products.Application.DTOs;

namespace Products.Application.Features.Products.GetProductById
{
    /// <summary>
    /// Read-only query that fetches a single product by identifier and projects it
    /// to a <see cref="ProductDto"/>. Returns <c>null</c> when no product with the
    /// given id exists, which the controller translates to HTTP 404.
    /// </summary>
    /// <param name="Id">Identifier of the product to fetch.</param>
    public sealed record GetProductByIdQuery(int Id) : IQuery<ProductDto?>;
}