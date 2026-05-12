using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Products.Application.Abstractions;
using RabbitMQ.Client;
using SharedKernel.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Products.Infrastructure.Messaging;

/// <summary>
/// RabbitMQ-backed implementation of <see cref="IEventBus"/>.
/// Used exclusively by <see cref="OutboxProcessor"/> to publish serialized
/// integration events from the Outbox table.
///
/// <para>
/// Exchange strategy: one <em>fanout</em> exchange per event type name.
/// Example: <c>ProductCreatedIntegrationEvent</c> → exchange
/// <c>"products.ProductCreatedIntegrationEvent"</c>.
/// Each consumer service binds its own durable queue to the exchange,
/// so adding a new consumer requires no change to the publisher.
/// </para>
/// </summary>
public sealed class RabbitMqEventBus(
    IConnection connection,
    ILogger<RabbitMqEventBus> logger)
    : IEventBus, IAsyncDisposable
{
    // One shared channel; IChannel is NOT thread-safe — OutboxProcessor
    // runs on a single background thread so this is safe.
    private IChannel? _channel;

    // ── IEventBus ────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken ct = default)
        where TEvent : IntegrationEvent
    {
        // Lazily create channel on first publish.
        _channel ??= await connection.CreateChannelAsync(cancellationToken: ct);

        // Derive exchange name from the event type: "products.ProductCreatedIntegrationEvent"
        var exchangeName = $"products.{typeof(TEvent).Name}";

        // Declare a durable fanout exchange. Idempotent — safe to call on every publish.
        // "fanout" routes every message to ALL bound queues, so multiple consumers
        // (e.g. Notifications AND Auth) each receive their own copy.
        await _channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Fanout,
            durable: true,   // survives broker restart
            autoDelete: false,
            cancellationToken: ct);

        // Serialize the event to UTF-8 JSON bytes.
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(integrationEvent));

        // Build message properties: persistent delivery so messages survive broker restart.
        var props = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            MessageId = integrationEvent.EventId.ToString(),
            // Timestamp is used by consumers for observability / dead-letter policies.
            Timestamp = new AmqpTimestamp(
                new DateTimeOffset(integrationEvent.OccurredOn).ToUnixTimeSeconds())
        };

        await _channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: string.Empty, // fanout ignores routing keys
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: ct);

        logger.LogInformation(
            "Published {EventType} [{EventId}] to exchange {Exchange}",
            typeof(TEvent).Name, integrationEvent.EventId, exchangeName);
    }

    // ── IAsyncDisposable ─────────────────────────────────────────────────────

    /// <summary>Disposes the AMQP channel (connection is shared and disposed by the DI container).</summary>
    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.DisposeAsync();
    }
}