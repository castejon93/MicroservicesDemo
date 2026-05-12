using Products.Application.Abstractions;
using Products.Domain.Events;
using Products.Infrastructure.Data;

namespace Products.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IDomainEventCollector"/>.
/// Uses the <see cref="ProductsDbContext"/> <c>ChangeTracker</c> to find all
/// tracked entities that implement <see cref="IHasDomainEvents"/>, extracts
/// their pending events, and clears them atomically.
///
/// Registered as <c>Scoped</c> in DI — one instance per HTTP request,
/// sharing the same <see cref="ProductsDbContext"/> lifetime.
/// </summary>
public sealed class EfDomainEventCollector(ProductsDbContext db) : IDomainEventCollector
{
    /// <inheritdoc/>
    public IReadOnlyList<IDomainEvent> CollectAndClear()
    {
        // Snapshot all domain events from every tracked aggregate root.
        var domainEvents = db.ChangeTracker
            .Entries<IHasDomainEvents>()
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToList();

        // Clear BEFORE returning so re-entrant calls don't double-dispatch.
        foreach (var entry in db.ChangeTracker.Entries<IHasDomainEvents>())
            entry.Entity.ClearDomainEvents();

        return domainEvents;
    }
}