using Products.Application.DTOs;

namespace Products.Application.Interfaces
{
    /// <summary>
    /// Product Service Interface
    /// 
    /// Defines all use cases related to Product management.
    /// By using an interface, we can:
    /// 1. Inject the service into controllers
    /// 2. Mock it for unit testing
    /// 3. Swap implementations if needed
    /// 
    /// This service represents the APPLICATION LAYER
    /// It orchestrates between:
    /// - Presentation Layer (Controllers) ↓ calls this service
    /// - Domain Layer ↑ implements business rules
    /// - Infrastructure Layer ↑ provides data access
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Use Case: Get all products
        /// Returns a list of all products in the system
        /// </summary>
        Task<List<ProductDto>> GetAllProductsAsync();

        /// <summary>
        /// Use Case: Get single product by ID
        /// Returns product details or null if not found
        /// </summary>
        Task<ProductDto?> GetProductByIdAsync(int id);

        /// <summary>
        /// Use Case: Create a new product
        /// Takes DTO from API, converts to Domain Entity, saves to database
        /// </summary>
        Task<int> CreateProductAsync(CreateProductDto productDto);

        /// <summary>
        /// Use Case: Update an existing product
        /// Finds product, updates properties, saves changes
        /// </summary>
        Task UpdateProductAsync(int id, UpdateProductDto productDto);

        /// <summary>
        /// Use Case: Delete a product
        /// Removes product from database
        /// </summary>
        Task DeleteProductAsync(int id);
    }
}
