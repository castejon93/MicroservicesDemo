using Products.Application.Cqrs;
using Products.Application.DTOs;

namespace Products.Application.Features.Products.GetProductById
{
    public sealed record GetProductByIdQuery(int Id) : IQuery<ProductDto?>;
}