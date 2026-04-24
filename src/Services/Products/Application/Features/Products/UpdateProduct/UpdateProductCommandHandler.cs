using MediatR;
using Products.Domain.Interfaces;

namespace Products.Application.Features.Products.UpdateProduct
{
    /// <summary>
    /// Handles <see cref="UpdateProductCommand"/> by loading the target aggregate,
    /// applying the requested changes through domain behaviors where available, and
    /// persisting the result. Execution runs inside the <c>TransactionBehavior</c>
    /// so the read-modify-write sequence commits atomically.
    /// </summary>
    public sealed class UpdateProductCommandHandler(IProductRepository repo)
        : IRequestHandler<UpdateProductCommand, bool>
    {
        /// <summary>
        /// Loads the product, reconciles its fields with the command payload, and saves
        /// the changes.
        /// </summary>
        /// <param name="request">Update payload carrying the target id and new field values.</param>
        /// <param name="cancellationToken">Token that cancels the repository calls.</param>
        /// <returns><c>true</c> when the product was found and updated; <c>false</c> when no product exists for the given id.</returns>
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
            // A positive diff tops up inventory; a negative diff is a reduction which the
            // entity may refuse if it would drop stock below zero.
            var diff = request.Stock - existing.Stock;
            if (diff > 0) existing.AddStock(diff);
            else if (diff < 0) existing.TryReduceStock(-diff);

            await repo.UpdateAsync(existing);
            return true;
        }
    }
}