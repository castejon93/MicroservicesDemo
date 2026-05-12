namespace SharedKernel.Events;

/// <summary>
/// Published by the <c>Products</c> service when a product is permanently removed.
/// Consumers should invalidate any local cache entry or projection for this product.
/// </summary>
/// <param name="ProductId">PK of the deleted product.</param>
public sealed record ProductDeletedIntegrationEvent(
    int ProductId) : IntegrationEvent;