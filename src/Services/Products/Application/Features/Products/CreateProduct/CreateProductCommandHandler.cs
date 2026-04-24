using MediatR;
using Products.Domain.Entities;
using Products.Domain.Interfaces;

namespace Products.Application.Features.Products.CreateProduct
{
    /// <summary>
    /// Handles <see cref="CreateProductCommand"/> by constructing a new
    /// <see cref="Product"/> aggregate through its domain constructor (which enforces
    /// invariants such as non-empty name and non-negative price/stock) and persisting
    /// it via the repository. Execution runs inside <c>TransactionBehavior</c>, so the
    /// insert commits atomically with the retrying execution strategy.
    /// </summary>
    public sealed class CreateProductCommandHandler(IProductRepository repo)
        : IRequestHandler<CreateProductCommand, int>
    {
        /// <summary>
        /// Creates and persists a new product, returning the identifier assigned by
        /// the database.
        /// </summary>
        /// <param name="request">Command carrying the name, optional description, price, and initial stock.</param>
        /// <param name="cancellationToken">Token linked to the request lifetime.</param>
        /// <returns>The identifier of the newly created product.</returns>
        public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            // Route input through the domain constructor so invariants are enforced
            // in one place rather than duplicated in the handler. Description is
            // modeled as required non-null on the entity; coalesce the optional
            // command field to an empty string to match.
            var product = new Product(
                request.Name,
                request.Description ?? string.Empty,
                request.Price,
                request.Stock);

            return await repo.AddAsync(product);
        }
    }
}
