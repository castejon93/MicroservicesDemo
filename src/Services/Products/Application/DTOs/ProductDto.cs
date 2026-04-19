namespace Products.Application.DTOs
{
    /// <summary>
    /// Product Data Transfer Object (DTO)
    /// 
    /// A DTO is used to transfer data between layers without exposing domain entities.
    /// 
    /// Why use DTOs?
    /// 1. SECURITY: We don't expose domain entities to API clients
    /// 2. DECOUPLING: If domain entity changes, clients don't break
    /// 3. FILTERING: Only expose properties that clients need to see
    /// 4. OPTIMIZATION: Can flatten/reshape data for specific use cases
    /// 
    /// Flow: Database → Domain Entity → DTO → JSON → Client Browser
    ///
    /// This particular DTO is for READ operations (getting product data)
    /// </summary>
    public class ProductDto
    {
        /// <summary>
        /// Unique identifier for the product (from database)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Product name to display
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Product description for marketing/info
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Price in currency units
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Available stock count
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// When was this product created (for audit trail)
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When was this product last updated (for audit trail)
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
