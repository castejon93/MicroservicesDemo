using MediatR;

namespace Products.Application.Cqrs;

/// <summary>
/// Marker interface identifying a write/mutation request that returns a payload.
/// Requests implementing <see cref="ICommand{TResponse}"/> are wrapped in a database
/// transaction by <c>TransactionBehavior</c>; queries are not.
/// </summary>
/// <typeparam name="TResponse">The value returned by the command handler.</typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse> { }

/// <summary>
/// Marker interface for a write/mutation request that does not produce a response value
/// (modeled as MediatR's <see cref="Unit"/>). Also transactional.
/// </summary>
public interface ICommand : IRequest<Unit> { }

/// <summary>
/// Marker interface identifying a read-only request. Queries deliberately bypass the
/// transactional behavior since they do not mutate state.
/// </summary>
/// <typeparam name="TResponse">The value returned by the query handler.</typeparam>
public interface IQuery<out TResponse> : IRequest<TResponse> { }