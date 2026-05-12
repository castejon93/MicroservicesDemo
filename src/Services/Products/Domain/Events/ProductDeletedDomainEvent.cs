namespace Products.Domain.Events;

/// <summary>
/// Raised when a product is removed by <c>DeleteProductCommandHandler</c>.
/// </summary>
public sealed record ProductDeletedDomainEvent(int ProductId) : IDomainEvent;