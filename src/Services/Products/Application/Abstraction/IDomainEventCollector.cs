using Products.Domain.Events;

namespace Products.Application.Abstractions;

/// <summary>
/// Abstraction that collects domain events from all currently tracked aggregates.
/// Lives in the <em>Application</em> layer so <see cref="DomainEventBehavior{TRequest,TResponse}"/>
/// can depend on it without referencing EF Core or <c>ProductsDbContext</c> directly.
///
/// The Infrastructure layer provides the concrete implementation
/// (<c>EfDomainEventCollector</c>) which reads from the EF Core <c>ChangeTracker</c>.
/// This preserves the Clean Architecture Dependency Rule: Application → Domain only,
/// never Application → Infrastructure.
/// </summary>
public interface IDomainEventCollector
{
    /// <summary>
    /// Returns all domain events accumulated by tracked aggregate roots,
    /// then clears them to prevent double-dispatch.
    /// </summary>
    /// <returns>
    /// A snapshot of all pending <see cref="IDomainEvent"/> instances raised since
    /// the last call. The collection is cleared on the aggregates after this returns.
    /// </returns>
    IReadOnlyList<IDomainEvent> CollectAndClear();
}