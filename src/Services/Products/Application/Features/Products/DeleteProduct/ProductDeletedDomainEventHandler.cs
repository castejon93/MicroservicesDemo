using MediatR;
using Products.Application.Abstractions;
using Products.Domain.Events;
using SharedKernel.Events;

namespace Products.Application.Features.Products.DeleteProduct;

/// <summary>
/// Reacts to <see cref="ProductDeletedDomainEvent"/> by appending a
/// <see cref="ProductDeletedIntegrationEvent"/> to the Outbox via <see cref="IOutboxRepository"/>.
/// Runs inside the same EF Core transaction as <c>DeleteProductCommand</c>.
/// </summary>
public sealed class ProductDeletedDomainEventHandler(IOutboxRepository outbox)
    : INotificationHandler<ProductDeletedDomainEvent>
{
    /// <inheritdoc/>
    public async Task Handle(ProductDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new ProductDeletedIntegrationEvent(notification.ProductId);

        await outbox.AppendAsync(integrationEvent, cancellationToken);
    }
}