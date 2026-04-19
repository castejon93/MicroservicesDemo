using MediatR;
using Products.Domain.Interfaces;

namespace Products.Application.Features.Products.UpdateProduct
{
    public sealed class UpdateProductCommandHandler(IProductRepository repo)
        : IRequestHandler<UpdateProductCommand, bool>
    {
        public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var existing = await repo.GetByIdAsync(request.Id);
            if (existing is null) return false;

            // Mutate through domain behaviors where available; fall back to direct property
            // assignment for fields the entity exposes as settable (Name/Description/Price).
            existing.Name = request.Name;
            existing.Description = request.Description;
            existing.Price = request.Price;

            // Reconcile stock using the domain methods to honor invariants.
            var diff = request.Stock - existing.Stock;
            if (diff > 0) existing.AddStock(diff);
            else if (diff < 0) existing.TryReduceStock(-diff);

            await repo.UpdateAsync(existing);
            return true;
        }
    }
}