namespace Products.Domain.Events;

/// <summary>
/// Implemented by aggregate roots that accumulate domain events.
/// The <c>DomainEventBehavior</c> in the MediatR pipeline queries this interface
/// on all EF-tracked entities after <c>SaveChangesAsync</c> completes but before
/// the DB transaction commits, then dispatches each event via MediatR.
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>Events raised since the last <see cref="ClearDomainEvents"/> call.</summary>
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Removes all accumulated events. Called by the behavior after dispatch
    /// to prevent double-firing if the aggregate is reused within the same scope.
    /// </summary>
    void ClearDomainEvents();
}