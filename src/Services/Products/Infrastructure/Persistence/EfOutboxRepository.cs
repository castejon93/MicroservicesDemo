using System.Text.Json;
using Products.Application.Abstractions;
using Products.Infrastructure.Data;
using Products.Infrastructure.Outbox;
using SharedKernel.Events;

namespace Products.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IOutboxRepository"/>.
/// Serializes the integration event to JSON and appends an <see cref="OutboxMessage"/>
/// row to the <see cref="ProductsDbContext"/> change tracker.
///
/// <para>
/// <b>No <c>SaveChangesAsync</c> call is made here.</b> The row is staged in the
/// EF change tracker and flushed together with the domain data when
/// <c>TransactionBehavior</c> commits the ambient transaction — guaranteeing atomicity.
/// </para>
///
/// Registered as <c>Scoped</c> in DI so it shares the same
/// <see cref="ProductsDbContext"/> instance as the rest of the request pipeline.
/// </summary>
public sealed class EfOutboxRepository(ProductsDbContext db) : IOutboxRepository
{
    /// <inheritdoc/>
    public async Task AppendAsync<TEvent>(TEvent integrationEvent, CancellationToken ct = default)
        where TEvent : IntegrationEvent
    {
        // Serialize the integration event payload to JSON.
        // EventType stores the full CLR name so OutboxProcessor can deserialize
        // back to the correct concrete type when publishing to RabbitMQ.
        var outboxMessage = new OutboxMessage
        {
            EventType = integrationEvent.GetType().AssemblyQualifiedName!,
            Payload = JsonSerializer.Serialize(integrationEvent)
        };

        // Stage the row in the EF change tracker.
        // SaveChangesAsync is intentionally NOT called here — the transaction
        // boundary above us in the pipeline flushes everything atomically.
        await db.OutboxMessages.AddAsync(outboxMessage, ct);
    }
}