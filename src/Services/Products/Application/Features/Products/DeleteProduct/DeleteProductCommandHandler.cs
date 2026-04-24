using MediatR;
using Products.Domain.Interfaces;

namespace Products.Application.Features.Products.DeleteProduct
{
    /// <summary>
    /// Handles <see cref="DeleteProductCommand"/> by verifying the product exists and
    /// then issuing a delete through the repository. The existence check is done first
    /// so the caller gets a distinct <c>false</c> result for missing rows instead of
    /// an EF concurrency exception.
    /// </summary>
    public sealed class DeleteProductCommandHandler(IProductRepository repo)
        : IRequestHandler<DeleteProductCommand, bool>
    {
        /// <summary>
        /// Verifies the product exists and removes it.
        /// </summary>
        /// <param name="request">Command carrying the identifier to delete.</param>
        /// <param name="cancellationToken">Token that cancels the repository calls.</param>
        /// <returns><c>true</c> when the product was found and deleted; otherwise <c>false</c>.</returns>
        public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            // Existence check keeps the semantics explicit for the caller (404 vs 204).
            var existing = await repo.GetByIdAsync(request.Id);
            if (existing is null) return false;

            await repo.DeleteAsync(request.Id);
            return true;
        }
    }
}