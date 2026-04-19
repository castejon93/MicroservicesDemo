using Products.Application.DTOs;
using Products.Application.Interfaces;
using Products.Domain.Entities;
using Products.Domain.Interfaces;

namespace Products.Application.Services
{
    /// <summary>
    /// Product Service Implementation
    /// 
    /// This is where APPLICATION LOGIC lives:
    /// - Converting DTOs to Domain Entities
    /// - Orchestrating repository calls
    /// - Implementing business workflows
    /// - Error handling
    /// 
    /// Key Pattern: Dependency Injection
    /// We inject IProductRepository interface, not concrete class.
    /// This means:
    /// 1. We don't create the repository ourselves (bad practice)
    /// 2. Someone else (Program.cs) provides the implementation
    /// 3. Easy to test - can inject mock repository
    /// 4. Easy to change - just change the registration in Program.cs
    /// </summary>
    public class ProductService : IProductService
    {
        // DEPENDENCY INJECTION: This field holds the repository
        // Notice: It's an INTERFACE, not a concrete class
        // This is the KEY to loosely coupled code
        private readonly IProductRepository _repository;

        /// <summary>
        /// Constructor with dependency injection
        /// The IoC container (Program.cs) will create an instance
        /// and pass it to this constructor automatically
        /// 
        /// Example flow:
        /// 1. User requests /api/products
        /// 2. ASP.NET creates ProductsController
        /// 3. Controller constructor needs IProductService
        /// 4. ASP.NET creates ProductService
        /// 5. ProductService constructor needs IProductRepository
        /// 6. ASP.NET looks up how to create IProductRepository (finds ProductRepository)
        /// 7. Creates ProductRepository and passes to ProductService
        /// 8. Everything is wired up automatically!
        /// </summary>
        public ProductService(IProductRepository repository)
        {
            // Store the injected repository
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        // USE CASE IMPLEMENTATIONS

        /// <summary>
        /// Retrieves all products and converts them to DTOs
        /// 
        /// Application Logic Flow:
        /// 1. Call repository to get all Product entities from database
        /// 2. For each entity, convert to DTO (transform)
        /// 3. Return DTOs to controller
        /// 
        /// Why convert to DTOs?
        /// - Clients don't need to see internal domain logic
        /// - Protects domain entities from modification through API
        /// - Can customize data returned without changing domain
        /// </summary>
        public async Task<List<ProductDto>> GetAllProductsAsync()
        {
            try
            {
                // STEP 1: Get entities from repository (database)
                var products = await _repository.GetAllAsync();

                // STEP 2: Convert each entity to DTO
                // Using LINQ Select: transform Product → ProductDto
                var productDtos = products
                    .Select(p => MapProductToDto(p))
                    .ToList();

                // STEP 3: Return DTOs to caller (controller)
                return productDtos;
            }
            catch (Exception ex)
            {
                // ERROR HANDLING: Log and re-throw
                // In real app, would log this to logging service
                throw new ApplicationException("Error retrieving products", ex);
            }
        }

        /// <summary>
        /// Retrieves a single product by ID
        /// 
        /// Application Logic:
        /// 1. Query repository for product with ID
        /// 2. If found: convert to DTO and return
        /// 3. If not found: return null (controller handles 404)
        /// </summary>
        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            // Validate input
            if (id <= 0)
                throw new ArgumentException("Product ID must be greater than 0", nameof(id));

            try
            {
                // Get from repository
                var product = await _repository.GetByIdAsync(id);

                // If not found, return null
                if (product == null)
                    return null;

                // Convert to DTO and return
                return MapProductToDto(product);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error retrieving product {id}", ex);
            }
        }

        /// <summary>
        /// Creates a new product
        /// 
        /// Application Logic:
        /// 1. Validate input DTO
        /// 2. Convert DTO to Domain Entity (creating new Product)
        /// 3. Entity constructor validates business rules
        /// 4. Send to repository to save
        /// 5. Return the new product ID
        /// 
        /// This is a USE CASE: A complete business transaction
        /// </summary>
        public async Task<int> CreateProductAsync(CreateProductDto productDto)
        {
            // VALIDATION: Check if DTO has required data
            if (productDto == null)
                throw new ArgumentNullException(nameof(productDto));

            if (string.IsNullOrWhiteSpace(productDto.Name))
                throw new ArgumentException("Product name is required", nameof(productDto.Name));

            if (productDto.Price < 0)
                throw new ArgumentException("Product price cannot be negative", nameof(productDto.Price));

            if (productDto.Stock < 0)
                throw new ArgumentException("Product stock cannot be negative", nameof(productDto.Stock));

            try
            {
                // CREATE: Build domain entity from DTO
                // The Product constructor contains business logic validation
                var product = new Product(
                    productDto.Name,
                    productDto.Description,
                    productDto.Price,
                    productDto.Stock
                );

                // SAVE: Send to repository (which saves to database)
                var newProductId = await _repository.AddAsync(product);

                // RETURN: The ID of newly created product
                return newProductId;
            }
            catch (ArgumentException)
            {
                // Business logic error - re-throw as is
                throw;
            }
            catch (Exception ex)
            {
                // Database or infrastructure error
                throw new ApplicationException("Error creating product", ex);
            }
        }

        /// <summary>
        /// Updates an existing product
        /// 
        /// Application Logic:
        /// 1. Validate input
        /// 2. Retrieve existing product from database
        /// 3. Check if product exists
        /// 4. Update properties with new values
        /// 5. Update timestamp
        /// 6. Save changes
        /// 
        /// This is a CRITICAL PATTERN:
        /// We fetch the entity first, modify it, then save.
        /// This ensures we're modifying a REAL entity that exists in DB.
        /// </summary>
        public async Task UpdateProductAsync(int id, UpdateProductDto productDto)
        {
            // VALIDATION
            if (id <= 0)
                throw new ArgumentException("Product ID must be greater than 0", nameof(id));

            if (productDto == null)
                throw new ArgumentNullException(nameof(productDto));

            if (string.IsNullOrWhiteSpace(productDto.Name))
                throw new ArgumentException("Product name is required", nameof(productDto.Name));

            if (productDto.Price < 0)
                throw new ArgumentException("Product price cannot be negative", nameof(productDto.Price));

            try
            {
                // RETRIEVE: Get the existing product
                var product = await _repository.GetByIdAsync(id);

                // CHECK: Does it exist?
                if (product == null)
                    throw new KeyNotFoundException($"Product with ID {id} not found");

                // UPDATE: Modify the properties
                product.Name = productDto.Name;
                product.Description = productDto.Description;
                product.Price = productDto.Price;
                product.Stock = productDto.Stock;

                // AUDIT: Update timestamp
                product.UpdatedAt = DateTime.UtcNow;

                // SAVE: Send updated entity back to repository
                await _repository.UpdateAsync(product);
            }
            catch (KeyNotFoundException)
            {
                // Expected error - re-throw
                throw;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error updating product {id}", ex);
            }
        }

        /// <summary>
        /// Deletes a product
        /// 
        /// Simple use case: Just tell repository to delete by ID
        /// Repository will handle "product doesn't exist" gracefully
        /// </summary>
        public async Task DeleteProductAsync(int id)
        {
            // Validate input
            if (id <= 0)
                throw new ArgumentException("Product ID must be greater than 0", nameof(id));

            try
            {
                // DELETE: Tell repository to remove this product
                await _repository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error deleting product {id}", ex);
            }
        }

        // HELPER METHODS

        /// <summary>
        /// Maps a Domain Entity to a DTO
        /// This conversion happens in application layer
        /// 
        /// Why separate method?
        /// 1. REUSABILITY: Used by multiple methods (GetAll, GetById)
        /// 2. MAINTAINABILITY: If DTO structure changes, update one place
        /// 3. READABILITY: Clear what data is being transformed
        /// </summary>
        private static ProductDto MapProductToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}
