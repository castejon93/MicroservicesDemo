using Products.Application.Cqrs;

namespace Products.Application.Features.Products.UpdateProduct
{
    public sealed record UpdateProductCommand(
        int Id,
        string Name,
        string? Description,
        decimal Price,
        int Stock) : ICommand<bool>;
}