namespace Products.Application.DTOs
{
    /// <summary>
    /// Product Update Request DTO
    /// Used when CLIENT sends data to UPDATE an existing product
    /// Note: No Id (comes from URL), No timestamps (server manages them)
    /// </summary>
    public class UpdateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
