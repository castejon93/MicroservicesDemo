using MediatR;
using Products.Application.Abstractions;
using Products.Domain.Events;
using SharedKernel.Events;

namespace Products.Application.Features.Products.UpdateProduct;

/// <summary>
/// Reacts to <see cref="ProductUpdatedDomainEvent"/> by appending a
/// <see cref="ProductUpdatedIntegrationEvent"/> to the Outbox via <see cref="IOutboxRepository"/>.
/// Runs inside the same EF Core transaction as <c>UpdateProductCommand</c>.
/// </summary>
public sealed class ProductUpdatedDomainEventHandler(IOutboxRepository outbox)
    : INotificationHandler<ProductUpdatedDomainEvent>
{
    /// <inheritdoc/>
    public async Task Handle(ProductUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new ProductUpdatedIntegrationEvent(
            ProductId: notification.ProductId,
            Name: notification.Name,
            Price: notification.Price,
            Stock: notification.Stock);

        await outbox.AppendAsync(integrationEvent, cancellationToken);
    }
}