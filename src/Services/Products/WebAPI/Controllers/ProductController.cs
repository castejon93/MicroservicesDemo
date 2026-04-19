using Products.Application.DTOs;
using Products.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Products.WebAPI.Controllers
{
    /// <summary>
    /// Products Controller - Presentation Layer (API Endpoint)
    /// 
    /// Responsibilities:
    /// 1. Receive HTTP requests (GET, POST, PUT, DELETE)
    /// 2. Delegate to ProductService for business logic
    /// 3. Convert service responses to HTTP responses
    /// 4. Return appropriate HTTP status codes
    /// 
    /// What it does NOT do:
    /// ❌ Access database directly
    /// ❌ Implement business logic (that's service's job)
    /// ✓ Only handles HTTP concerns
    /// 
    /// Architecture Benefits:
    /// - Controller is THIN and FOCUSED on HTTP only
    /// - All business logic is in ProductService
    /// - Easy to test with mocked service
    /// - Business logic is isolated from HTTP concerns
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        /// <summary>
        /// Constructor with dependency injection
        /// 
        /// ASP.NET automatically:
        /// 1. Creates instance of ProductService
        /// 2. Passes it to this constructor
        /// 3. Every request gets a new controller with service
        /// </summary>
        public ProductsController(IProductService productService)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        }

        /// <summary>
        /// HTTP GET /api/products
        /// 
        /// Retrieves all products
        /// 
        /// Response:
        /// - 200 OK: Returns list of products as JSON
        /// - 500 Internal Server Error: If service throws exception
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<ProductDto>>> GetAll()
        {
            try
            {
                var result = await _productService.GetAllProductsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving products");
            }
        }

        /// <summary>
        /// HTTP GET /api/products/{id}
        /// 
        /// Retrieves a single product by ID
        /// 
        /// Parameters:
        /// - id: Product ID from URL (e.g., /api/products/5)
        /// 
        /// Response:
        /// - 200 OK: Returns the product as JSON
        /// - 404 Not Found: If product doesn't exist
        /// - 400 Bad Request: If id is invalid
        /// - 500 Internal Server Error: If service throws exception
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductDto>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest("Product ID must be greater than 0");

                var result = await _productService.GetProductByIdAsync(id);

                if (result == null)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving product");
            }
        }

        /// <summary>
        /// HTTP POST /api/products
        /// 
        /// Creates a new product
        /// 
        /// Request Body:
        /// {
        ///   "name": "Laptop",
        ///   "description": "High performance laptop",
        ///   "price": 999.99,
        ///   "stock": 50
        /// }
        /// 
        /// Response:
        /// - 201 Created: Product created successfully
        ///   Location header: /api/products/{id}
        /// - 400 Bad Request: Validation failed
        /// - 500 Internal Server Error: Database error
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<object>> Create(CreateProductDto createProductDto)
        {
            try
            {
                var productId = await _productService.CreateProductAsync(createProductDto);

                return CreatedAtAction(nameof(GetById), new { id = productId },
                    new { id = productId, message = "Product created successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating product");
            }
        }

        /// <summary>
        /// HTTP PUT /api/products/{id}
        /// 
        /// Updates an existing product
        /// 
        /// Request:
        /// - URL: PUT /api/products/5
        /// - Body: UpdateProductDto with updated values
        /// 
        /// Response:
        /// - 200 OK: Update successful
        /// - 400 Bad Request: Validation failed
        /// - 404 Not Found: Product doesn't exist
        /// - 500 Internal Server Error: Database error
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<object>> UpdateProduct(int id, UpdateProductDto updateProductDto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest("Product ID must be greater than 0");

                await _productService.UpdateProductAsync(id, updateProductDto);

                return Ok(new { message = "Product updated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating product");
            }
        }

        /// <summary>
        /// HTTP DELETE /api/products/{id}
        /// 
        /// Deletes a product
        /// 
        /// Request:
        /// - URL: DELETE /api/products/5
        /// 
        /// Response:
        /// - 200 OK: Deletion successful
        /// - 404 Not Found: Product doesn't exist
        /// - 500 Internal Server Error: Database error
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<object>> DeleteProduct(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest("Product ID must be greater than 0");

                await _productService.DeleteProductAsync(id);

                return Ok(new { message = "Product deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting product");
            }
        }
    }
}
