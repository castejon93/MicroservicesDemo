using Products.Domain.Entities;
using Products.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Products.Infrastructure.Data;

namespace Products.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        // CHANGE: Use ApplicationDbContext instead of DbContext
        private readonly ProductsDbContext _context;
        private readonly DbSet<Product> _dbSet;

        /// <summary>
        /// Constructor - receives ApplicationDbContext from dependency injection
        /// NOW it matches what's registered in Program.cs
        /// </summary>
        public ProductRepository(ProductsDbContext context)  // ← CHANGE HERE
        {
            // VALIDATE: Ensure context was provided
            _context = context ?? throw new ArgumentNullException(nameof(context));

            // GET THE DbSet: Get reference to the Products table
            _dbSet = context.Set<Product>();
        }

        // Rest of the code remains the same...
        public async Task<List<Product>> GetAllAsync()
        {
            try
            {
                var products = await _dbSet.ToListAsync();
                return products;
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Error retrieving products from database", ex);
            }
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            try
            {
                var product = await _dbSet.FindAsync(id);
                return product;
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Error retrieving product {id} from database", ex);
            }
        }

        public async Task<int> AddAsync(Product product)
        {
            try
            {
                if (product == null)
                    throw new ArgumentNullException(nameof(product));

                await _dbSet.AddAsync(product);
                await _context.SaveChangesAsync();
                return product.Id;
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Error adding product to database", ex);
            }
        }

        public async Task UpdateAsync(Product product)
        {
            try
            {
                if (product == null)
                    throw new ArgumentNullException(nameof(product));

                if (product.Id <= 0)
                    throw new ArgumentException("Invalid product ID", nameof(product.Id));

                var existingProduct = await _dbSet.FindAsync(product.Id);
                if (existingProduct == null)
                    throw new KeyNotFoundException($"Product {product.Id} not found");

                _dbSet.Update(product);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Error updating product {product.Id} in database", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Invalid product ID", nameof(id));

                var product = await _dbSet.FindAsync(id);
                if (product == null)
                    return;

                _dbSet.Remove(product);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Error deleting product {id} from database", ex);
            }
        }
    }
}