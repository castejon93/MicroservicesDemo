using Products.Application.Cqrs;

namespace Products.Application.Features.Products.DeleteProduct
{
    /// <summary>
    /// Write intent to permanently remove a product from the catalog.
    /// Returns <c>true</c> when the product existed and was deleted, <c>false</c>
    /// when no product with the given <paramref name="Id"/> exists.
    /// </summary>
    /// <param name="Id">Identifier of the product to delete. Must be positive.</param>
    public sealed record DeleteProductCommand(int Id) : ICommand<bool>;
}