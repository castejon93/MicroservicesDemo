using Products.Application.Cqrs;

namespace Products.Application.Features.Products.CreateProduct
{
    /// <summary>
    /// Write intent to create a new product in the catalog. Mirrors
    /// <c>CreateProductDto</c> but lives in the Application layer as the canonical
    /// command executed by MediatR. The handler returns the identity of the newly
    /// persisted product.
    /// </summary>
    /// <param name="Name">Display name of the product. Required; max length 200.</param>
    /// <param name="Description">Optional long-form description.</param>
    /// <param name="Price">Unit price. Must be non-negative.</param>
    /// <param name="Stock">Initial on-hand inventory. Must be non-negative.</param>
    public sealed record CreateProductCommand(
        string Name,
        string? Description,
        decimal Price,
        int Stock) : ICommand<int>;   // returns the new Product Id
}