namespace SharedKernel.Events;

/// <summary>
/// Published by the <c>Products</c> service when a new product is added to the catalog.
/// Other services (e.g. Notifications, Search indexer) can react without querying Products.
/// </summary>
/// <param name="ProductId">Database PK of the newly created product.</param>
/// <param name="Name">Product display name.</param>
/// <param name="Price">Unit price at time of creation.</param>
/// <param name="Stock">Initial on-hand inventory.</param>
public sealed record ProductCreatedIntegrationEvent(
    int ProductId,
    string Name,
    decimal Price,
    int Stock) : IntegrationEvent;