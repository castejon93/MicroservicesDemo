using Products.Domain.Entities;

namespace Products.Domain.Interfaces
{
    /// <summary>
    /// Repository Interface - defines the contract for data access operations
    /// This is an abstraction that allows the domain layer to NOT depend on database implementation
    /// 
    /// Benefits:
    /// 1. Decouples domain from database technology
    /// 2. Enables dependency injection
    /// 3. Makes testing easier (can mock this interface)
    /// 4. If we change databases, only implementation changes, not this interface
    /// </summary>
    public interface IProductRepository
    {
        /// <summary>
        /// Retrieves all products from the database asynchronously
        /// </summary>
        /// <returns>List of all Product entities</returns>
        Task<List<Product>> GetAllAsync();

        /// <summary>
        /// Retrieves a single product by its ID
        /// </summary>
        /// <param name="id">The product ID to search for</param>
        /// <returns>Product if found, null if not found</returns>
        Task<Product?> GetByIdAsync(int id);

        /// <summary>
        /// Adds a new product to the database
        /// </summary>
        /// <param name="product">The product entity to add</param>
        /// <returns>The ID of the newly created product</returns>
        Task<int> AddAsync(Product product);

        /// <summary>
        /// Updates an existing product in the database
        /// </summary>
        /// <param name="product">The product entity with updated values</param>
        Task UpdateAsync(Product product);

        /// <summary>
        /// Deletes a product from the database by ID
        /// </summary>
        /// <param name="id">The ID of the product to delete</param>
        Task DeleteAsync(int id);
    }
}