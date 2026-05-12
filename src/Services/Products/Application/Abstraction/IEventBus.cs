using SharedKernel.Events;

namespace Products.Application.Abstractions;

/// <summary>
/// Abstraction for publishing integration events to an external message broker.
/// Lives in the <em>Application</em> layer so handlers can depend on it without
/// referencing RabbitMQ or any infrastructure concern.
///
/// The concrete implementation (<c>RabbitMqEventBus</c>) is registered in
/// <c>Program.cs</c> and injected at runtime.
///
/// <para>
/// <b>Note:</b> In most cases you will NOT call this directly from command handlers.
/// Instead, command handlers raise domain events, and the <c>DomainEventBehavior</c>
/// dispatches them. Domain event handlers then write to the Outbox, and the
/// <c>OutboxProcessor</c> calls <see cref="PublishAsync{TEvent}"/> from outside the
/// transaction. This keeps publishing decoupled from the transaction boundary.
/// </para>
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes an integration event to the message broker.
    /// </summary>
    /// <typeparam name="TEvent">
    ///   Concrete integration event type; must derive from <see cref="IntegrationEvent"/>.
    /// </typeparam>
    /// <param name="integrationEvent">The event payload to publish.</param>
    /// <param name="ct">Cancellation token.</param>
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken ct = default)
        where TEvent : IntegrationEvent;
}