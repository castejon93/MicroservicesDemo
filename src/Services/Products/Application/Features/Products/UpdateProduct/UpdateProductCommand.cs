using Products.Application.Cqrs;

namespace Products.Application.Features.Products.UpdateProduct
{
    /// <summary>
    /// Write intent to update an existing product. Returns <c>true</c> when the
    /// target product was found and updated, <c>false</c> when no product with the
    /// given <paramref name="Id"/> exists.
    /// </summary>
    /// <param name="Id">Identifier of the product to update. Must be positive.</param>
    /// <param name="Name">New display name. Required; max length 200.</param>
    /// <param name="Description">New optional description.</param>
    /// <param name="Price">New unit price. Must be non-negative.</param>
    /// <param name="Stock">Desired stock level; reconciled via the product's domain methods.</param>
    public sealed record UpdateProductCommand(
        int Id,
        string Name,
        string? Description,
        decimal Price,
        int Stock) : ICommand<bool>;
}