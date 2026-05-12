using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Products.Application.Abstractions;
using Products.Infrastructure.Data;
using SharedKernel.Events;

namespace Products.Infrastructure.Outbox;

/// <summary>
/// Background service that polls the Outbox table every
/// <see cref="PollingInterval"/> and publishes pending integration events to RabbitMQ.
///
/// <para>
/// <b>At-least-once delivery guarantee:</b> if the service crashes after publishing
/// but before marking the row as processed, the row will be re-published on the next
/// poll. Consumers MUST be idempotent (use <c>EventId</c> to deduplicate).
/// </para>
///
/// <para>
/// <b>Ordering:</b> events are published in <see cref="OutboxMessage.CreatedOn"/> order,
/// preserving the sequence in which domain operations occurred.
/// </para>
/// </summary>
public sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    IEventBus eventBus,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    /// <summary>How often the processor checks for new outbox messages.</summary>
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);

    /// <summary>Maximum rows processed per poll cycle to bound execution time.</summary>
    private const int BatchSize = 20;

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OutboxProcessor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Log and continue — a transient DB or RabbitMQ error should not
                // kill the processor; the next poll will retry.
                logger.LogError(ex, "OutboxProcessor encountered an error during batch processing.");
            }

            // Wait before next poll. Use Task.Delay so cancellation is respected.
            await Task.Delay(PollingInterval, stoppingToken);
        }

        logger.LogInformation("OutboxProcessor stopped.");
    }

    /// <summary>
    /// Fetches one batch of unprocessed outbox messages, publishes each to RabbitMQ,
    /// and marks them as processed. Uses a fresh DI scope so the DbContext lifetime
    /// is bounded to this method call.
    /// </summary>
    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        // Create a new scope per batch — DbContext is Scoped and must not be
        // shared across background ticks (it holds a live SQL connection).
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();

        // ── Fetch unprocessed rows ───────────────────────────────────────────
        var messages = await db.OutboxMessages
            .Where(m => m.ProcessedOn == null)
            .OrderBy(m => m.CreatedOn)   // preserve causal order
            .Take(BatchSize)
            .ToListAsync(ct);

        if (messages.Count == 0) return;

        logger.LogInformation("OutboxProcessor: processing {Count} message(s).", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                // ── Deserialize and publish ──────────────────────────────────
                // Resolve the CLR type from the stored name so we can call
                // the correct generic overload of PublishAsync<TEvent>.
                var eventType = Type.GetType(message.EventType)
                    ?? throw new InvalidOperationException(
                        $"Cannot resolve event type '{message.EventType}'.");

                // Deserialize to the concrete integration event.
                var integrationEvent = (IntegrationEvent)JsonSerializer
                    .Deserialize(message.Payload, eventType)!;

                // Publish via reflection to call the generic method with the correct TEvent.
                // A source-generator or type-switch approach can replace this for hot paths.
                await (Task)typeof(IEventBus)
                    .GetMethod(nameof(IEventBus.PublishAsync))!
                    .MakeGenericMethod(eventType)
                    .Invoke(eventBus, [integrationEvent, ct])!;

                // ── Mark as processed ────────────────────────────────────────
                message.ProcessedOn = DateTime.UtcNow;

                logger.LogInformation(
                    "OutboxProcessor: published {EventType} [{Id}].",
                    message.EventType, message.Id);
            }
            catch (Exception ex)
            {
                // Store the error for observability. The row remains unprocessed
                // and will be retried on the next poll cycle.
                message.Error = ex.Message;
                logger.LogError(ex,
                    "OutboxProcessor: failed to publish message {Id} ({EventType}).",
                    message.Id, message.EventType);
            }
        }

        // Persist ProcessedOn stamps and any Error updates in one round-trip.
        await db.SaveChangesAsync(ct);
    }
}