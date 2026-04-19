// ============================================================
// FILE: src/Services/Products/Products.Infrastructure/Data/ProductsDbContext.cs
// PURPOSE: Database context for Products microservice
// LAYER: Infrastructure Layer
// DATABASE: Separate database for Products
// ============================================================

using Products.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Products.Infrastructure.Data
{
    /// <summary>
    /// Products Database Context
    /// 
    /// SEPARATE database from Auth microservice!
    /// 
    /// Database: ProductsDb (SQL Server)
    /// Tables: Products
    /// 
    /// This enforces microservice data isolation:
    /// - Products service owns product data
    /// - Auth service owns user data
    /// - No direct database joins between services
    /// </summary>
    public class ProductsDbContext : DbContext
    {
        public ProductsDbContext(DbContextOptions<ProductsDbContext> options)
            : base(options)
        {
        }

        // ============================================
        // DATABASE TABLES
        // ============================================

        /// <summary>
        /// Products table - product catalog data
        /// </summary>
        public DbSet<Product> Products { get; set; } = null!;

        /// <summary>
        /// Configure entity mappings
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.Price)
                    .HasPrecision(18, 2); // 18 digits, 2 decimal places
            });

            // Seed data
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Sample Product",
                    Description = "A sample product for testing",
                    Price = 29.99m,
                    Stock = 100,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}