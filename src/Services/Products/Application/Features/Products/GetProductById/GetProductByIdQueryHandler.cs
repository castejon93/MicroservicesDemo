using MediatR;
using Products.Application.DTOs;
using Products.Domain.Interfaces;

namespace Products.Application.Features.Products.GetProductById
{
    /// <summary>
    /// Handles <see cref="GetProductByIdQuery"/> by loading the entity via the
    /// repository and mapping it to a <see cref="ProductDto"/> so domain types do not
    /// leak out of the Application layer.
    /// </summary>
    public sealed class GetProductByIdQueryHandler(IProductRepository repo)
        : IRequestHandler<GetProductByIdQuery, ProductDto?>
    {
        /// <summary>
        /// Fetches the product and projects it to a DTO.
        /// </summary>
        /// <param name="request">Query carrying the identifier to look up.</param>
        /// <param name="cancellationToken">Token that cancels the repository call.</param>
        /// <returns>The product DTO, or <c>null</c> when the product does not exist.</returns>
        public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var p = await repo.GetByIdAsync(request.Id);
            if (p is null) return null;

            // Manual projection keeps the Application layer free of AutoMapper dependencies
            // and makes the shape of the DTO explicit for API consumers.
            return new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock
            };
        }
    }
}