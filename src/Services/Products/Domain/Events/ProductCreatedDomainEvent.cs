namespace Products.Domain.Events;

/// <summary>
/// Raised inside <see cref="Products.Domain.Entities.Product"/> when a new product
/// is successfully constructed and ready to be persisted.
/// The handler (<c>ProductCreatedDomainEventHandler</c>) converts this to a
/// <see cref="SharedKernel.Events.ProductCreatedIntegrationEvent"/> and writes it
/// to the Outbox table within the same transaction.
/// </summary>
/// <param name="ProductId">Transient entity id; populated after EF assigns the PK.</param>
/// <param name="Name">Product name carried forward for the integration event payload.</param>
/// <param name="Price">Unit price at creation time.</param>
/// <param name="Stock">Initial stock quantity.</param>
public sealed record ProductCreatedDomainEvent(
    int ProductId,
    string Name,
    decimal Price,
    int Stock) : IDomainEvent;