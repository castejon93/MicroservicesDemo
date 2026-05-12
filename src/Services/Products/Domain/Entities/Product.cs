using Products.Domain.Events;

namespace Products.Domain.Entities;

/// <summary>
/// Product aggregate root. Owns all business rules for a catalog product.
/// Implements <see cref="IHasDomainEvents"/> so the MediatR pipeline can
/// dispatch domain events after the entity is persisted.
/// </summary>
public class Product : IHasDomainEvents
{
    // ── Private backing field for domain events ──────────────────────────────
    // Events are stored privately so only the aggregate itself decides when to raise them.
    // The behavior/UoW reads them via the interface after SaveChanges.
    private readonly List<IDomainEvent> _domainEvents = [];

    // ── IHasDomainEvents ─────────────────────────────────────────────────────

    /// <inheritdoc/>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <inheritdoc/>
    public void ClearDomainEvents() => _domainEvents.Clear();

    // ── Identity ─────────────────────────────────────────────────────────────

    /// <summary>Primary key assigned by the database.</summary>
    public int Id { get; set; }

    /// <summary>Required display name; max 200 characters.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional long-form description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Unit price; must be non-negative.</summary>
    public decimal Price { get; set; }

    /// <summary>Current on-hand inventory; must be non-negative.</summary>
    public int Stock { get; set; }

    /// <summary>UTC timestamp of when this product was first persisted.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp of the most recent field mutation; null when never updated.</summary>
    public DateTime? UpdatedAt { get; set; }

    // ── Constructors ─────────────────────────────────────────────────────────

    /// <summary>Parameterless constructor required by Entity Framework Core.</summary>
    public Product() { }

    /// <summary>
    /// Creates a valid product and immediately raises a
    /// <see cref="ProductCreatedDomainEvent"/>.
    /// The event is held in <see cref="DomainEvents"/> until after
    /// <c>SaveChangesAsync</c> assigns the real PK, at which point
    /// <c>DomainEventBehavior</c> dispatches it.
    /// </summary>
    /// <param name="name">Non-empty display name.</param>
    /// <param name="description">Optional description; stored as empty string when null.</param>
    /// <param name="price">Non-negative unit price.</param>
    /// <param name="stock">Non-negative initial inventory.</param>
    /// <exception cref="ArgumentException">Thrown when invariants are violated.</exception>
    public Product(string name, string description, decimal price, int stock)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty.", nameof(name));
        if (price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));
        if (stock < 0)
            throw new ArgumentException("Stock cannot be negative.", nameof(stock));

        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
        CreatedAt = DateTime.UtcNow;

        // ── Raise domain event ──────────────────────────────────────────────
        // Id is 0 here (not yet assigned by the DB). The behavior dispatches
        // the event AFTER SaveChangesAsync, so by then EF has populated Id.
        // The handler reads entity.Id, not the event field, when building the
        // integration event — see ProductCreatedDomainEventHandler.
        _domainEvents.Add(new ProductCreatedDomainEvent(Id, Name, Price, Stock));
    }

    // ── Business methods ─────────────────────────────────────────────────────

    /// <summary>
    /// Applies a set of field updates and raises a <see cref="ProductUpdatedDomainEvent"/>.
    /// Call this from <c>UpdateProductCommandHandler</c> instead of setting properties directly
    /// so the domain event is always emitted when data changes.
    /// </summary>
    public void Update(string name, string description, decimal price, int newStock)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty.", nameof(name));
        if (price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));

        // Reconcile stock using existing invariant methods.
        var diff = newStock - Stock;
        if (diff > 0) AddStock(diff);
        if (diff < 0) TryReduceStock(-diff);

        Name = name;
        Description = description;
        Price = price;
        UpdatedAt = DateTime.UtcNow;

        _domainEvents.Add(new ProductUpdatedDomainEvent(Id, Name, Price, Stock));
    }

    /// <summary>
    /// Signals that this product is about to be deleted.
    /// Call before <c>repo.DeleteAsync</c> so the event is dispatched in the same transaction.
    /// </summary>
    public void MarkAsDeleted()
        => _domainEvents.Add(new ProductDeletedDomainEvent(Id));

    /// <summary>Increases on-hand inventory by <paramref name="quantity"/> units.</summary>
    /// <param name="quantity">Must be positive.</param>
    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity to add must be positive.", nameof(quantity));
        Stock += quantity;
    }

    /// <summary>
    /// Attempts to reduce on-hand inventory. Returns <c>false</c> when stock would go negative.
    /// </summary>
    /// <param name="quantity">Units to deduct; must be positive.</param>
    /// <returns><c>true</c> if the deduction succeeded; otherwise <c>false</c>.</returns>
    public bool TryReduceStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity to reduce must be positive.", nameof(quantity));
        if (Stock < quantity) return false;
        Stock -= quantity;
        return true;
    }
}