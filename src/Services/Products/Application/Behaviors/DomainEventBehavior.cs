using MediatR;
using Products.Application.Abstractions;
using Products.Application.Cqrs;

namespace Products.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that dispatches domain events raised by EF-tracked
/// aggregate roots after <c>SaveChangesAsync</c> but <em>before</em> the DB transaction
/// commits.
///
/// <para>Pipeline order (outermost → innermost):</para>
/// <code>
///   LoggingBehavior
///     └─ ValidationBehavior
///          └─ TransactionBehavior       ← opens DB transaction
///               └─ DomainEventBehavior  ← THIS: fires INSIDE the transaction
///                    └─ CommandHandler  ← mutates aggregates, calls SaveChanges
/// </code>
///
/// <para>
/// Depends only on <see cref="IDomainEventCollector"/> (Application abstraction)
/// and MediatR's <see cref="IPublisher"/> — zero Infrastructure references,
/// fully compliant with the Clean Architecture Dependency Rule.
/// </para>
/// </summary>
public sealed class DomainEventBehavior<TRequest, TResponse>(
    IDomainEventCollector collector, // ✅ Application abstraction — no Infrastructure reference
    IPublisher publisher)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    /// <inheritdoc/>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        // ── 1. Execute the command handler ───────────────────────────────────
        // Runs the real business logic. The handler mutates aggregates and
        // calls SaveChangesAsync via the repository — PKs are assigned by here.
        var response = await next();

        // ── 2. Collect and clear domain events via the abstraction ───────────
        // IDomainEventCollector hides the EF ChangeTracker from this layer.
        // Events are cleared on the aggregates immediately to prevent double-dispatch.
        var domainEvents = collector.CollectAndClear();

        // ── 3. Dispatch each domain event via MediatR ────────────────────────
        // IPublisher resolves all INotificationHandler<TEvent> implementations.
        // Each handler (e.g. ProductCreatedDomainEventHandler) writes one
        // OutboxMessage row — still inside the open EF transaction from TransactionBehavior.
        foreach (var domainEvent in domainEvents)
            await publisher.Publish(domainEvent, ct);

        return response;
    }
}