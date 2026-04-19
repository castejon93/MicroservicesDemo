namespace Products.Domain.Entities
{
    /// <summary>
    /// Product Entity - represents the core business domain object
    /// 
    /// An Entity is:
    /// - The heart of your application's business logic
    /// - Has an identity (Id) that uniquely identifies it
    /// - Should contain ONLY business rules related to a product
    /// - Should NOT contain infrastructure concerns (like DbContext)
    /// 
    /// This class is pure domain logic and has NO dependencies on:
    /// - Database (EntityFramework)
    /// - External services
    /// - HTTP frameworks
    /// </summary>
    public class Product
    {
        // PROPERTIES
        // Primary Key - uniquely identifies this product
        public int Id { get; set; }

        // Required: Product name
        public string Name { get; set; } = string.Empty;

        // Optional: Product description
        public string Description { get; set; } = string.Empty;

        // Product price in currency (decimal for money accuracy)
        public decimal Price { get; set; }

        // Current inventory stock level
        public int Stock { get; set; }

        // Audit: When was this product created?
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Audit: When was this product last modified?
        public DateTime? UpdatedAt { get; set; }

        // CONSTRUCTORS

        /// <summary>
        /// Parameterless constructor required by Entity Framework
        /// </summary>
        public Product() { }

        /// <summary>
        /// Constructor for creating a new Product with required properties
        /// Using constructor ensures we always have valid initial values
        /// This is better than allowing property initialization in any order
        /// 
        /// Example: var product = new Product("Laptop", "High performance laptop", 999.99m, 50);
        /// </summary>
        public Product(string name, string description, decimal price, int stock)
        {
            // Validate inputs to enforce business rules
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name cannot be empty", nameof(name));

            if (price < 0)
                throw new ArgumentException("Product price cannot be negative", nameof(price));

            if (stock < 0)
                throw new ArgumentException("Stock quantity cannot be negative", nameof(stock));

            // Assign properties
            Name = name;
            Description = description;
            Price = price;
            Stock = stock;
            CreatedAt = DateTime.UtcNow; // Always use UTC for consistency across time zones
        }

        // BUSINESS METHODS
        // These methods contain business logic specific to a Product

        /// <summary>
        /// Reduces stock when a product is sold
        /// Business Rule: Cannot sell more than available stock
        /// </summary>
        public bool TryReduceStock(int quantity)
        {
            // Check if we have enough stock
            if (quantity <= 0)
                return false;

            if (Stock < quantity)
                return false; // Not enough stock available

            Stock -= quantity;
            UpdatedAt = DateTime.UtcNow;
            return true;
        }

        /// <summary>
        /// Increases stock when new inventory arrives
        /// </summary>
        public void AddStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Cannot add negative or zero quantity", nameof(quantity));

            Stock += quantity;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Checks if product is in stock
        /// </summary>
        public bool IsInStock => Stock > 0;
    }
}