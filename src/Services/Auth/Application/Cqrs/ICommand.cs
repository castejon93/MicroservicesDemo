using MediatR;

namespace Auth.Application.Cqrs;

/// <summary>
/// Marker interface for a write / mutation request that returns <typeparamref name="TResponse"/>.
/// Handlers are wrapped by <c>TransactionBehavior</c> and <c>ValidationBehavior</c>
/// in the MediatR pipeline.
/// </summary>
/// <typeparam name="TResponse">The type returned by the command handler.</typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse> { }

/// <summary>
/// Marker interface for a write / mutation request that produces no payload response (<see cref="Unit"/>).
/// </summary>
public interface ICommand : IRequest<Unit> { }

/// <summary>
/// Marker interface for a read-only request that returns <typeparamref name="TResponse"/>.
/// Queries bypass <c>TransactionBehavior</c> because they do not mutate state.
/// </summary>
/// <typeparam name="TResponse">The type returned by the query handler.</typeparam>
public interface IQuery<out TResponse> : IRequest<TResponse> { }