namespace Products.Domain.Events;

/// <summary>
/// Raised when a product's fields are mutated by <c>UpdateProductCommandHandler</c>.
/// </summary>
public sealed record ProductUpdatedDomainEvent(
    int ProductId,
    string Name,
    decimal Price,
    int Stock) : IDomainEvent;