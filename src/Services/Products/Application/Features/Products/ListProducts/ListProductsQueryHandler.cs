using MediatR;
using Products.Application.DTOs;
using Products.Domain.Interfaces;

namespace Products.Application.Features.Products.ListProducts
{
    /// <summary>
    /// Handles <see cref="ListProductsQuery"/> by loading all products via the
    /// repository and projecting the aggregates to <see cref="ProductDto"/> instances.
    /// </summary>
    public sealed class ListProductsQueryHandler(IProductRepository repo)
        : IRequestHandler<ListProductsQuery, IReadOnlyList<ProductDto>>
    {
        /// <summary>
        /// Fetches every product and maps the result to a read-only list of DTOs.
        /// </summary>
        /// <param name="request">The query instance (carries no parameters).</param>
        /// <param name="cancellationToken">Token that cancels the repository call.</param>
        /// <returns>A read-only list of product DTOs, possibly empty.</returns>
        public async Task<IReadOnlyList<ProductDto>> Handle(ListProductsQuery request, CancellationToken cancellationToken)
        {
            var products = await repo.GetAllAsync();

            // Manual projection to keep domain entities inside the Domain layer.
            return products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock
            }).ToList();
        }
    }
}