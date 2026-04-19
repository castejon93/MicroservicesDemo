using Products.Application.Cqrs;

namespace Products.Application.Features.Products.DeleteProduct
{
    public sealed record DeleteProductCommand(int Id) : ICommand<bool>;
}