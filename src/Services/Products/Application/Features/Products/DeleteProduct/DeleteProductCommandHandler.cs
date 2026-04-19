using MediatR;
using Products.Domain.Interfaces;

namespace Products.Application.Features.Products.DeleteProduct
{
    public sealed class DeleteProductCommandHandler(IProductRepository repo)
        : IRequestHandler<DeleteProductCommand, bool>
    {
        public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var existing = await repo.GetByIdAsync(request.Id);
            if (existing is null) return false;

            await repo.DeleteAsync(request.Id);
            return true;
        }
    }
}