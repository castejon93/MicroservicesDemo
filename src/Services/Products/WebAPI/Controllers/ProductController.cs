using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Products.Application.DTOs;
using Products.Application.Features.Products.CreateProduct;
using Products.Application.Features.Products.DeleteProduct;
using Products.Application.Features.Products.GetProductById;
using Products.Application.Features.Products.ListProducts;
using Products.Application.Features.Products.UpdateProduct;

namespace Products.WebAPI.Controllers
{
    /// <summary>
    /// HTTP adapter for the product catalog. Every endpoint dispatches to a MediatR
    /// command or query handler — the controller deliberately contains no business
    /// logic, only request shaping and status-code mapping. All endpoints require a
    /// valid JWT issued by the Auth service (tokens are shared via identical
    /// <c>JwtSettings</c> in configuration).
    /// </summary>
    [ApiController]
    [Route("api/products")]
    [Authorize] // All endpoints require a valid JWT; override per-action if needed.
    public sealed class ProductController(ISender mediator) : ControllerBase
    {
        /// <summary>
        /// Returns every product in the catalog.
        /// </summary>
        /// <param name="ct">Cancellation token linked to the HTTP request lifetime.</param>
        /// <returns><c>200 OK</c> with the (possibly empty) list of products.</returns>
        // GET api/products
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(CancellationToken ct)
            => Ok(await mediator.Send(new ListProductsQuery(), ct));

        /// <summary>
        /// Retrieves a single product by identifier.
        /// </summary>
        /// <param name="id">The product identifier from the route.</param>
        /// <param name="ct">Cancellation token linked to the HTTP request lifetime.</param>
        /// <returns><c>200 OK</c> with the product, or <c>404 Not Found</c> if it does not exist.</returns>
        // GET api/product/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductDto>> GetById(int id, CancellationToken ct)
        {
            var product = await mediator.Send(new GetProductByIdQuery(id), ct);
            return product is null ? NotFound() : Ok(product);
        }

        /// <summary>
        /// Creates a new product in the catalog.
        /// </summary>
        /// <param name="dto">Create payload with name, optional description, price, and stock.</param>
        /// <param name="ct">Cancellation token linked to the HTTP request lifetime.</param>
        /// <returns><c>201 Created</c> with the new identifier and a <c>Location</c> header pointing to <see cref="GetById"/>.</returns>
        // POST api/product
        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] CreateProductDto dto, CancellationToken ct)
        {
            var id = await mediator.Send(
                new CreateProductCommand(dto.Name, dto.Description, dto.Price, dto.Stock),
                ct);

            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        /// <summary>
        /// Updates an existing product. The route identifier is authoritative and any
        /// id embedded in the request body is ignored.
        /// </summary>
        /// <param name="id">The product identifier from the route.</param>
        /// <param name="dto">Update payload with the new field values.</param>
        /// <param name="ct">Cancellation token linked to the HTTP request lifetime.</param>
        /// <returns><c>204 No Content</c> on success; <c>404 Not Found</c> when the product does not exist.</returns>
        // PUT api/product/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto, CancellationToken ct)
        {
            // Route id is authoritative — ignore any id embedded in the body.
            var ok = await mediator.Send(
                new UpdateProductCommand(id, dto.Name, dto.Description, dto.Price, dto.Stock),
                ct);

            return ok ? NoContent() : NotFound();
        }

        /// <summary>
        /// Permanently deletes a product.
        /// </summary>
        /// <param name="id">The product identifier from the route.</param>
        /// <param name="ct">Cancellation token linked to the HTTP request lifetime.</param>
        /// <returns><c>204 No Content</c> on success; <c>404 Not Found</c> when the product does not exist.</returns>
        // DELETE api/product/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var ok = await mediator.Send(new DeleteProductCommand(id), ct);
            return ok ? NoContent() : NotFound();
        }
    }
}