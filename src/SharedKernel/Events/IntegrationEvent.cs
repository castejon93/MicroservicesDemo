using System;

namespace SharedKernel.Events;

/// <summary>
/// Base record for every integration event published over RabbitMQ.
/// Integration events cross service boundaries — they are the public API
/// of your messaging contract. Once published, treat them as immutable:
/// add properties in a backwards-compatible way (new nullable/defaulted fields)
/// and version the event name when breaking changes are unavoidable.
/// </summary>
/// <param name="EventId">
///   Unique identifier for this specific event occurrence.
///   Consumers use this for idempotency checks (deduplicate redeliveries).
/// </param>
/// <param name="OccurredOn">
///   UTC timestamp of when the domain action that triggered this event occurred.
///   Always UTC so consumers in different time zones interpret it consistently.
/// </param>
public abstract record IntegrationEvent(
    Guid EventId,
    DateTime OccurredOn)
{
    /// <summary>
    /// Parameterless factory: generates a new <see cref="EventId"/> and sets
    /// <see cref="OccurredOn"/> to <see cref="DateTime.UtcNow"/> automatically.
    /// Use this constructor from derived records so callers never forget to set them.
    /// </summary>
    protected IntegrationEvent() : this(Guid.NewGuid(), DateTime.UtcNow) { }
}