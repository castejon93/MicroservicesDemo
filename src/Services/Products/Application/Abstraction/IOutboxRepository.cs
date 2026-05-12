using SharedKernel.Events;

namespace Products.Application.Abstractions;

/// <summary>
/// Abstraction for appending integration events to the Outbox store.
/// Lives in the <em>Application</em> layer so domain event handlers can write
/// to the Outbox without referencing <c>OutboxMessage</c>, <c>ProductsDbContext</c>,
/// or any other Infrastructure concern.
///
/// The Infrastructure layer provides the concrete implementation
/// (<c>EfOutboxRepository</c>) which knows about EF Core and the Outbox table.
///
/// <para>
/// The implementation must write within the <em>same</em> EF Core transaction
/// that the <c>TransactionBehavior</c> opens, so the Outbox row and the domain
/// data change commit atomically — this is the core guarantee of the Outbox pattern.
/// </para>
/// </summary>
public interface IOutboxRepository
{
    /// <summary>
    /// Serializes <paramref name="integrationEvent"/> and appends it to the
    /// Outbox as a pending message. Does <b>not</b> call <c>SaveChangesAsync</c>
    /// — the caller's transaction boundary controls the final flush.
    /// </summary>
    /// <typeparam name="TEvent">
    ///   Concrete integration event type; must derive from <see cref="IntegrationEvent"/>.
    /// </typeparam>
    /// <param name="integrationEvent">The event payload to persist.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AppendAsync<TEvent>(TEvent integrationEvent, CancellationToken ct = default)
        where TEvent : IntegrationEvent;
}