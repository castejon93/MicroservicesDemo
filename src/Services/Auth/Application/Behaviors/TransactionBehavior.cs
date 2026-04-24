using Auth.Application.Abstractions;
using Auth.Application.Cqrs;
using MediatR;

namespace Auth.Application.Behaviors
{
    /// <summary>
    /// Wraps every <see cref="ICommand{TResponse}"/> handler in a retry-safe
    /// EF Core transaction via <see cref="IUnitOfWork.ExecuteInTransactionAsync{TResult}"/>.
    /// Read-only <c>IQuery</c> requests do not satisfy the generic constraint, so MediatR
    /// skips this behavior for them automatically.
    /// </summary>
    public sealed class TransactionBehavior<TRequest, TResponse>(IUnitOfWork uow)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : ICommand<TResponse>
    {
        /// <summary>
        /// Delegates execution of <paramref name="next"/> to <see cref="IUnitOfWork"/>
        /// so that the entire handler runs inside a single database transaction.
        /// If the execution strategy retries on a transient failure, the full
        /// delegate is re-invoked — ensure command handlers are idempotent.
        /// </summary>
        public Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken ct)
            => uow.ExecuteInTransactionAsync(_ => next(), ct);
    }
}