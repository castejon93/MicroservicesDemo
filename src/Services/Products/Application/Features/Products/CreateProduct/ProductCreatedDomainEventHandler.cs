using MediatR;
using Products.Application.Abstractions;
using Products.Domain.Events;
using SharedKernel.Events;

namespace Products.Application.Features.Products.CreateProduct;

/// <summary>
/// Reacts to <see cref="ProductCreatedDomainEvent"/> by translating it into a
/// <see cref="ProductCreatedIntegrationEvent"/> and persisting it to the Outbox
/// via <see cref="IOutboxRepository"/>.
///
/// Runs <em>inside the same EF Core transaction</em> as the originating
/// <c>CreateProductCommand</c> — if anything fails, both the product insert and
/// this Outbox row are rolled back atomically.
///
/// Zero Infrastructure references: depends only on Application abstractions
/// and Domain/SharedKernel types, fully compliant with Clean Architecture.
/// </summary>
public sealed class ProductCreatedDomainEventHandler(IOutboxRepository outbox)
    : INotificationHandler<ProductCreatedDomainEvent>
{
    /// <inheritdoc/>
    public async Task Handle(ProductCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        // Translate the domain event into the public integration event contract.
        // By the time this handler runs, SaveChanges has already been called by
        // the repository, so notification.ProductId carries the real DB-assigned PK.
        var integrationEvent = new ProductCreatedIntegrationEvent(
            ProductId: notification.ProductId,
            Name: notification.Name,
            Price: notification.Price,
            Stock: notification.Stock);

        // Delegate serialization and storage to the Outbox abstraction.
        // No knowledge of OutboxMessage, DbContext, or JSON serialization here.
        await outbox.AppendAsync(integrationEvent, cancellationToken);
    }
}