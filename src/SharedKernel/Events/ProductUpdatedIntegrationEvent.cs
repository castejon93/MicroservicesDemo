namespace SharedKernel.Events;

/// <summary>
/// Published by the <c>Products</c> service when an existing product's details change.
/// Consumers holding a local cache or projection of product data should update their copy.
/// </summary>
public sealed record ProductUpdatedIntegrationEvent(
    int ProductId,
    string Name,
    decimal Price,
    int Stock) : IntegrationEvent;