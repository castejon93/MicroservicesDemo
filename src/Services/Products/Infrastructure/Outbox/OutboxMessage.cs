namespace Products.Infrastructure.Outbox;

/// <summary>
/// Represents a serialized integration event waiting to be published to RabbitMQ.
/// Rows are inserted within the <em>same</em> EF Core transaction as the originating
/// domain command, so the event is guaranteed to exist in the DB if (and only if)
/// the business data change also committed.
///
/// The <see cref="OutboxProcessor"/> background service reads unprocessed rows,
/// publishes them to RabbitMQ, and stamps <see cref="ProcessedOn"/> to mark completion.
/// This guarantees <em>at-least-once delivery</em> — consumers must be idempotent.
/// </summary>
public sealed class OutboxMessage
{
    /// <summary>Surrogate PK; also serves as the idempotency key sent to consumers.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Fully-qualified CLR type name of the integration event (e.g.
    /// <c>SharedKernel.Events.ProductCreatedIntegrationEvent</c>).
    /// Used by <see cref="OutboxProcessor"/> to resolve the correct
    /// RabbitMQ exchange/routing key.
    /// </summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>
    /// JSON-serialized integration event payload.
    /// Serialized with <c>System.Text.Json</c>; polymorphism is handled via
    /// <see cref="EventType"/> so no <c>$type</c> discriminator is embedded.
    /// </summary>
    public string Payload { get; init; } = string.Empty;

    /// <summary>UTC timestamp when this row was inserted.</summary>
    public DateTime CreatedOn { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// UTC timestamp when the event was successfully published to RabbitMQ.
    /// <see langword="null"/> means the row is still pending.
    /// </summary>
    public DateTime? ProcessedOn { get; set; }

    /// <summary>
    /// Last error message from a failed publish attempt.
    /// Useful for debugging stuck messages; does not block retry.
    /// </summary>
    public string? Error { get; set; }
}