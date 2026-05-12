using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedKernel.Events;

namespace Products.Infrastructure.Consumers;

/// <summary>
/// RabbitMQ consumer that listens for <see cref="UserRegisteredIntegrationEvent"/>
/// published by the <c>Auth</c> microservice.
///
/// <para>
/// <b>What it can do:</b> warm up a local read-model of known users,
/// send a welcome email, provision default products, etc. This example
/// just logs the event to illustrate the wiring.
/// </para>
///
/// <para>
/// <b>Idempotency:</b> check <c>EventId</c> against a processed-events table
/// before acting to handle RabbitMQ redeliveries (at-least-once semantics).
/// </para>
/// </summary>
public sealed class UserRegisteredConsumer(
    IConnection connection,
    IServiceScopeFactory scopeFactory,
    ILogger<UserRegisteredConsumer> logger) : BackgroundService
{
    // ── RabbitMQ topology constants ──────────────────────────────────────────
    // These must match the names used by the Auth service publisher.
    private const string ExchangeName = "auth.UserRegisteredIntegrationEvent";
    private const string QueueName = "products.user-registered";

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        // Declare the exchange (must match what Auth service declares).
        await channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        // Declare this service's own durable queue.
        // Using a named queue (not exclusive/auto-delete) survives service restarts.
        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        // Bind the queue to the exchange — any message on the exchange
        // is routed to this queue (fanout ignores routing keys).
        await channel.QueueBindAsync(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: string.Empty,
            cancellationToken: stoppingToken);

        // prefetchCount = 1: process one message at a time to simplify
        // transactional handling. Increase for higher throughput.
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                // ── Deserialize ──────────────────────────────────────────────
                var json = Encoding.UTF8.GetString(ea.Body.Span);
                var evt = JsonSerializer.Deserialize<UserRegisteredIntegrationEvent>(json);

                if (evt is null)
                {
                    logger.LogWarning("Received null or unparseable UserRegisteredIntegrationEvent.");
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
                logger.LogError(ex, "Error handling UserRegisteredIntegrationEvent.");

                // Requeue for retry. In production, add a retry counter to
                // dead-letter after N attempts and avoid infinite loops.
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
    /// Business logic executed when a <see cref="UserRegisteredIntegrationEvent"/> arrives.
    /// Replace the log statement with your actual reaction (e.g. create a local user profile).
    /// </summary>
    private async Task HandleAsync(UserRegisteredIntegrationEvent evt, CancellationToken ct)
    {
        // Example: log for now; extend with real reactions.
        logger.LogInformation(
            "Products service received UserRegistered event: User {UserId} ({Username}) registered at {OccurredOn}.",
            evt.UserId, evt.Username, evt.OccurredOn);

        // TODO: e.g. create a UserProfile read-model, send welcome notification, etc.
        await Task.CompletedTask;
    }
}