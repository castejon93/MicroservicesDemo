using Products.Application.Abstractions;
using Products.Application.Cqrs;
using MediatR;

namespace Products.Application.Behaviors
{
    /// <summary>
    /// Wraps ICommand<TResponse> handlers in a retry-safe transaction.
    /// Queries don't satisfy the constraint, so MediatR skips this behavior for them.
    /// </summary>
    public sealed class TransactionBehavior<TRequest, TResponse>(IUnitOfWork uow)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : ICommand<TResponse>
    {
        public Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken ct)
            => uow.ExecuteInTransactionAsync(_ => next(), ct);
    }
}