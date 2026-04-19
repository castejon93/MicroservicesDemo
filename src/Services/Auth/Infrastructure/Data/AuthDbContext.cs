// ============================================================
// FILE: src/Services/Auth/Auth.Infrastructure/Data/AuthDbContext.cs
// PURPOSE: Database context for Auth microservice
// LAYER: Infrastructure Layer
// DATABASE: Separate database for Auth (microservice pattern)
// ============================================================

using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Data
{
    /// <summary>
    /// Auth Database Context
    /// 
    /// IMPORTANT: This is a SEPARATE database from Products!
    /// 
    /// Microservices Pattern:
    /// - Each microservice owns its data
    /// - No shared databases between services
    /// - Communication via APIs or message queues
    /// 
    /// Database: AuthDb (SQL Server)
    /// Tables: Users
    /// </summary>
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options)
            : base(options)
        {
        }

        // ============================================
        // DATABASE TABLES
        // ============================================

        /// <summary>
        /// Users table - authentication data only
        /// </summary>
        public DbSet<User> Users { get; set; } = null!;

        /// <summary>
        /// Configure entity mappings
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================
            // USER ENTITY CONFIGURATION
            // ============================================

            modelBuilder.Entity<User>(entity =>
            {
                // Table name
                entity.ToTable("Users");

                // Primary key
                entity.HasKey(e => e.Id);

                // Username: required, unique, max 50 chars
                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.HasIndex(e => e.Username).IsUnique();

                // Email: required, unique, max 100 chars
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();

                // Password hash: required
                entity.Property(e => e.PasswordHash)
                    .IsRequired();

                // Role: required, default "User"
                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasDefaultValue("User");
            });

            // ============================================
            // SEED DATA (Development only)
            // ============================================

            // Default admin user
            // Password: password123 (BCrypt hash)
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                Username = "admin01",
                Email = "admin@example.com",
                PasswordHash = "$2a$12$w/uSw0m7daZZnQRy/7vs9upf/k6TXPGv3K.avLrn0jfjyaXd.5pj2",
                FirstName = "Admin",
                LastName = "User",
                Role = "Admin",
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true
            });
        }
    }
}