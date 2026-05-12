using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedKernel.Events;

namespace Notifications.Infrastructure.Consumers;

/// <summary>
/// RabbitMQ consumer that listens for <see cref="ProductCreatedIntegrationEvent"/>
/// published by the <c>Products</c> microservice and reacts to it.
///
/// <para>
/// <b>Topology:</b>
/// The Products service declares a durable fanout exchange named
/// <c>"products.ProductCreatedIntegrationEvent"</c> and publishes every new product there.
/// This consumer declares its own durable queue <c>"notifications.product-created"</c>
/// and binds it to that exchange. Because the queue is durable and exclusively owned
/// by this service, messages accumulate here even when this service is offline —
/// nothing is lost during restarts or deployments.
/// </para>
///
/// <para>
/// <b>At-least-once delivery:</b>
/// RabbitMQ may redeliver a message if the consumer crashes before acknowledging it.
/// Implement idempotency in <see cref="HandleAsync"/> (e.g. check a processed-events
/// table keyed on <c>evt.EventId</c>) before performing side effects such as sending emails.
/// </para>
///
/// <para>
/// <b>How to extend:</b> Replace the log statement in <see cref="HandleAsync"/> with
/// real notification logic — send an email, push notification, update a feed, etc.
/// Inject additional services via the constructor using <see cref="IServiceScopeFactory"/>
/// to create scoped dependencies (DbContext, email sender, etc.) inside the handler.
/// </para>
/// </summary>
public sealed class ProductCreatedConsumer(
    IConnection connection,
    IServiceScopeFactory scopeFactory,
    ILogger<ProductCreatedConsumer> logger) : BackgroundService
{
    // ── RabbitMQ topology constants ──────────────────────────────────────────

    /// <summary>
    /// Must match exactly the exchange declared by Products' RabbitMqEventBus:
    ///   <c>$"products.{typeof(ProductCreatedIntegrationEvent).Name}"</c>
    /// </summary>
    private const string ExchangeName = "products.ProductCreatedIntegrationEvent";

    /// <summary>
    /// The name of the durable queue that belongs to this service.
    /// Convention: "{consuming-service}.{event-kebab-case}".
    /// Each consuming service uses a different queue name so they each get
    /// their own independent copy of every message (fanout semantics).
    /// </summary>
    private const string QueueName = "notifications.product-created";

    // ── BackgroundService ────────────────────────────────────────────────────

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Each consumer gets its own channel. Channels are NOT thread-safe,
        // but a BackgroundService runs on a single thread, so this is safe.
        var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        // ── 1. Declare the exchange ──────────────────────────────────────────
        // Declaring an exchange is idempotent — if it already exists with the
        // same parameters, this is a no-op. It's safe (and good practice) to
        // declare it here even though the Products service declared it first,
        // because this service might start before Products does.
        await channel.ExchangeDeclareAsync(
            exchange:   ExchangeName,
            type:       ExchangeType.Fanout, // every bound queue receives every message
            durable:    true,                // survives RabbitMQ broker restart
            autoDelete: false,               // not deleted when last consumer disconnects
            cancellationToken: stoppingToken);

        // ── 2. Declare this service's own durable queue ──────────────────────
        // This is the key step that makes messages persist.
        // Without a queue bound to the exchange, RabbitMQ drops all published messages.
        // With a durable named queue, messages accumulate here while this service
        // is offline and are delivered when it comes back up.
        await channel.QueueDeclareAsync(
            queue:      QueueName,
            durable:    true,   // queue survives broker restart (messages too, if persistent)
            exclusive:  false,  // multiple service instances can consume the same queue
            autoDelete: false,  // queue is NOT deleted when the last consumer disconnects
            cancellationToken: stoppingToken);

        // ── 3. Bind the queue to the exchange ────────────────────────────────
        // This tells RabbitMQ: "route every message on ExchangeName to QueueName".
        // Fanout exchanges ignore the routingKey — pass empty string by convention.
        await channel.QueueBindAsync(
            queue:      QueueName,
            exchange:   ExchangeName,
            routingKey: string.Empty,
            cancellationToken: stoppingToken);

        // ── 4. Quality-of-service ────────────────────────────────────────────
        // prefetchCount: 1 means RabbitMQ only sends this consumer ONE unacked message
        // at a time. The next message is not delivered until BasicAck is called.
        // This prevents a slow handler from accumulating a backlog in memory.
        // Increase to e.g. 10-50 for higher throughput once the handler is stable.
        await channel.BasicQosAsync(
            prefetchSize:  0,     // no limit on message size
            prefetchCount: 1,     // one in-flight message per consumer
            global:        false, // apply per-consumer (not per-channel)
            cancellationToken: stoppingToken);

        // ── 5. Set up the async event handler ───────────────────────────────
        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                // ── Deserialize ──────────────────────────────────────────────
                var json = Encoding.UTF8.GetString(ea.Body.Span);
                var evt  = JsonSerializer.Deserialize<ProductCreatedIntegrationEvent>(json);

                if (evt is null)
                {
                    logger.LogWarning(
                        "Received null or unparseable ProductCreatedIntegrationEvent. " +
                        "Discarding message (no requeue).");

                    // Nack without requeue — malformed message. If a dead-letter
                    // exchange (DLX) is configured on the queue, the message lands there.
                    await channel.BasicNackAsync(
                        deliveryTag: ea.DeliveryTag,
                        multiple:    false,
                        requeue:     false,
                        cancellationToken: stoppingToken);
                    return;
                }

                // ── Handle ───────────────────────────────────────────────────
                await HandleAsync(evt, stoppingToken);

                // ── Acknowledge ──────────────────────────────────────────────
                // Ack ONLY after successful processing. RabbitMQ will not remove the
                // message from the queue until it receives this ack. If the process
                // crashes between receiving and acking, RabbitMQ redelivers.
                await channel.BasicAckAsync(
                    deliveryTag: ea.DeliveryTag,
                    multiple:    false,
                    cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error handling ProductCreatedIntegrationEvent.");

                // Nack with requeue=true so RabbitMQ redelivers to any available consumer.
                // WARNING: this can cause infinite loops if the message is always bad.
                // In production, use a retry policy (e.g. Polly) + dead-letter after N attempts.
                await channel.BasicNackAsync(
                    deliveryTag: ea.DeliveryTag,
                    multiple:    false,
                    requeue:     true,
                    cancellationToken: stoppingToken);
            }
        };

        // ── 6. Start consuming ───────────────────────────────────────────────
        // autoAck: false — we send acks manually after successful processing.
        // With autoAck: true, RabbitMQ acks immediately on delivery, meaning
        // messages would be lost if the handler crashes before finishing.
        await channel.BasicConsumeAsync(
            queue:    QueueName,
            autoAck:  false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        logger.LogInformation(
            "ProductCreatedConsumer started. Listening on queue '{Queue}' " +
            "bound to exchange '{Exchange}'.",
            QueueName, ExchangeName);

        // Park this task until the host signals cancellation (app shutdown).
        // The consumer continues to fire ReceivedAsync events on the RabbitMQ
        // thread pool while this task is parked.
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    // ── Business logic ───────────────────────────────────────────────────────

    /// <summary>
    /// Executes the notification logic when a <see cref="ProductCreatedIntegrationEvent"/> arrives.
    /// </summary>
    /// <remarks>
    /// <b>Idempotency note:</b> RabbitMQ guarantees at-least-once delivery.
    /// Before sending a notification, check whether <c>evt.EventId</c> has already
    /// been processed (e.g. query a <c>ProcessedEvents</c> table) to avoid
    /// duplicate emails on redelivery.
    /// </remarks>
    private async Task HandleAsync(ProductCreatedIntegrationEvent evt, CancellationToken ct)
    {
        // Use scopeFactory to resolve scoped services (e.g. DbContext, email sender).
        // Example:
        //   await using var scope = scopeFactory.CreateAsyncScope();
        //   var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
        //   await emailSender.SendNewProductEmailAsync(evt.ProductId, evt.Name, ct);

        logger.LogInformation(
            "Notifications service received ProductCreated event: " +
            "Product {ProductId} ({Name}) — Price: {Price}, Stock: {Stock}. " +
            "EventId: {EventId}, OccurredOn: {OccurredOn}.",
            evt.ProductId, evt.Name, evt.Price, evt.Stock,
            evt.EventId,   evt.OccurredOn);

        // TODO: replace with real notification logic (email, push, SMS, feed update, etc.)
        await Task.CompletedTask;
    }
}
