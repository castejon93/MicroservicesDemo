using Products.Application.Cqrs;

namespace Products.Application.Features.Products.CreateProduct
{
    // Mirrors CreateProductDto but lives in the Application layer as a write intent.
    public sealed record CreateProductCommand(
        string Name,
        string? Description,
        decimal Price,
        int Stock) : ICommand<int>;   // returns the new Product Id
}