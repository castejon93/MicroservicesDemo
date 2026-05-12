using Microsoft.EntityFrameworkCore;
using Products.Domain.Entities;
using Products.Infrastructure.Outbox;

namespace Products.Infrastructure.Data;

/// <summary>
/// EF Core database context for the Products microservice.
/// Contains the product catalog and the Outbox table used for
/// reliable integration event delivery to RabbitMQ.
/// </summary>
public sealed class ProductsDbContext(DbContextOptions<ProductsDbContext> options)
    : DbContext(options)
{
    /// <summary>Product catalog table.</summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>
    /// Outbox table — stores serialized integration events pending publication.
    /// Written atomically with domain changes; read by <see cref="OutboxProcessor"/>.
    /// </summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── Product ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Price).HasPrecision(18, 2);
        });

        // ── OutboxMessage ────────────────────────────────────────────────────
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(m => m.Id);

            // Index on ProcessedOn so the processor efficiently queries
            // only unprocessed rows (WHERE ProcessedOn IS NULL).
            entity.HasIndex(m => m.ProcessedOn);

            entity.Property(m => m.EventType).IsRequired().HasMaxLength(500);
            entity.Property(m => m.Payload).IsRequired();
        });
    }
}