using MediatR;

namespace Products.Domain.Events;

/// <summary>
/// Marker interface for domain events.
/// Domain events are in-process notifications that express something meaningful
/// that happened inside the domain boundary. They are dispatched by MediatR
/// <em>inside</em> the same DB transaction as the originating command, so handlers
/// can write to the Outbox table atomically — no dual-write problem.
///
/// Contrast with integration events (<see cref="SharedKernel.Events.IntegrationEvent"/>)
/// which cross service boundaries via RabbitMQ.
/// </summary>
public interface IDomainEvent : INotification { }