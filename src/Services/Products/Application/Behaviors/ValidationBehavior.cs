using FluentValidation;
using MediatR;

/// <summary>
/// MediatR pipeline behavior that runs every registered FluentValidation
/// <see cref="IValidator{T}"/> for the incoming request and short-circuits the
/// pipeline with a <see cref="ValidationException"/> when any rule fails. Registered
/// between <c>LoggingBehavior</c> and <c>TransactionBehavior</c> so invalid input
/// is rejected before a DB transaction is opened.
/// </summary>
/// <typeparam name="TRequest">The MediatR request type being validated.</typeparam>
/// <typeparam name="TResponse">The response type returned by the handler.</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    /// <summary>
    /// Executes all validators in parallel, aggregates failures, and throws a
    /// <see cref="ValidationException"/> when any are found; otherwise forwards the
    /// request to the next pipeline component. The resulting exception is translated
    /// into an HTTP 400 response by <c>ValidationExceptionMiddleware</c>.
    /// </summary>
    /// <param name="request">The incoming MediatR request to validate.</param>
    /// <param name="next">Delegate that invokes the next behavior/handler.</param>
    /// <param name="ct">Cancellation token propagated to each validator.</param>
    /// <returns>The response from the downstream handler when validation passes.</returns>
    /// <exception cref="ValidationException">Thrown when one or more validators report failures.</exception>
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        // Fast-path: no validators registered for this request type.
        if (!validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);

        // Run validators concurrently and flatten the resulting failure lists.
        var failures = (await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, ct))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count > 0) throw new ValidationException(failures);
        return await next();
    }
}