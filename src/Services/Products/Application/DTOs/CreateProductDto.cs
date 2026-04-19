namespace Products.Application.DTOs
{
    /// <summary>
    /// Product Create Request DTO
    /// Used when CLIENT sends data to CREATE a new product
    /// Note: No Id (database generates it), No timestamps (server sets them)
    /// </summary>
    public class CreateProductDto
    {
        /// <summary>
        /// Product name (required)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Product description (optional)
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Initial price (required)
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Initial stock quantity (required)
        /// </summary>
        public int Stock { get; set; }
    }
}
