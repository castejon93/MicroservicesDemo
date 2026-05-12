using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedKernel.Events;

namespace Auth.Infrastructure.Consumers;

/// <summary>
/// RabbitMQ consumer that listens for <see cref="ProductCreatedIntegrationEvent"/>
/// published by the <c>Products</c> microservice.
///
/// <para>
/// <b>What it can do:</b> maintain a local product catalogue read-model,
/// trigger notifications, update pricing rules, etc. This example just logs
/// the event to illustrate the wiring.
/// </para>
///
/// <para>
/// <b>Idempotency:</b> check <c>EventId</c> against a processed-events table
/// before acting to handle RabbitMQ redeliveries (at-least-once semantics).
/// </para>
/// </summary>
public sealed class ProductCreatedConsumer(
    IConnection connection,
    IServiceScopeFactory scopeFactory,
    ILogger<ProductCreatedConsumer> logger) : BackgroundService
{
    // ── RabbitMQ topology constants ──────────────────────────────────────────
    // ExchangeName must match the name declared by Products' RabbitMqEventBus:
    //   $"products.{typeof(ProductCreatedIntegrationEvent).Name}"
    private const string ExchangeName = "products.ProductCreatedIntegrationEvent";
    private const string QueueName = "auth.product-created";

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        // Declare the exchange (must match what Products service declares).
        await channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        // Declare this service's own durable queue.
        // Using a named queue (not exclusive/auto-delete) survives service restarts
        // and accumulates messages while this service is offline.
        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        // Bind the queue to the exchange — fanout routes every message here.
        await channel.QueueBindAsync(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: string.Empty,
            cancellationToken: stoppingToken);

        // Process one message at a time to simplify transactional handling.
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                // ── Deserialize ──────────────────────────────────────────────
                var json = Encoding.UTF8.GetString(ea.Body.Span);
                var evt = JsonSerializer.Deserialize<ProductCreatedIntegrationEvent>(json);

                if (evt is null)
                {
                    logger.LogWarning("Received null or unparseable ProductCreatedIntegrationEvent.");
                    // Nack without requeue — bad message, send to dead-letter queue.
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false,
                        cancellationToken: stoppingToken);
                    return;
                }

                // ── Handle ───────────────────────────────────────────────────
                await HandleAsync(evt, stoppingToken);

                // ── Acknowledge ──────────────────────────────────────────────
                // Only ack AFTER successful processing. If HandleAsync throws,
                // we nack with requeue=true so RabbitMQ redelivers.
                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false,
                    cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error handling ProductCreatedIntegrationEvent.");

                // Requeue for retry. In production, add a retry counter to
                // dead-letter after N attempts to avoid infinite loops.
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true,
                    cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false, // manual ack — we control exactly when to ack
            consumer: consumer,
            cancellationToken: stoppingToken);

        // Block until the service is stopped.
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    /// <summary>
    /// Business logic executed when a <see cref="ProductCreatedIntegrationEvent"/> arrives.
    /// Replace the log statement with your actual reaction (e.g. update a local product read-model).
    /// </summary>
    private async Task HandleAsync(ProductCreatedIntegrationEvent evt, CancellationToken ct)
    {
        logger.LogInformation(
            "Auth service received ProductCreated event: Product {ProductId} ({Name}) " +
            "at price {Price} with stock {Stock}.",
            evt.ProductId, evt.Name, evt.Price, evt.Stock);

        // TODO: e.g. update a local product catalogue, trigger notifications, etc.
        await Task.CompletedTask;
    }
}