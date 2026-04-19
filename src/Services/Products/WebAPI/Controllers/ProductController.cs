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
    [ApiController]
    [Route("api/products")]
    [Authorize] // All endpoints require a valid JWT; override per-action if needed.
    public sealed class ProductController(ISender mediator) : ControllerBase
    {
        // GET api/products
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(CancellationToken ct)
            => Ok(await mediator.Send(new ListProductsQuery(), ct));

        // GET api/product/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductDto>> GetById(int id, CancellationToken ct)
        {
            var product = await mediator.Send(new GetProductByIdQuery(id), ct);
            return product is null ? NotFound() : Ok(product);
        }

        // POST api/product
        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] CreateProductDto dto, CancellationToken ct)
        {
            var id = await mediator.Send(
                new CreateProductCommand(dto.Name, dto.Description, dto.Price, dto.Stock),
                ct);

            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

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

        // DELETE api/product/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var ok = await mediator.Send(new DeleteProductCommand(id), ct);
            return ok ? NoContent() : NotFound();
        }
    }
}