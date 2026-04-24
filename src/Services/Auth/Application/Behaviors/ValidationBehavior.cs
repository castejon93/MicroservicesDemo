using FluentValidation;
using MediatR;

/// <summary>
/// MediatR pipeline behavior that runs all registered <see cref="IValidator{TRequest}"/>
/// validators for the incoming <typeparamref name="TRequest"/> before the handler executes.
/// </summary>
/// <remarks>
/// Pipeline order in <c>Program.cs</c>: Logging → <b>Validation</b> → Transaction.
/// Validation short-circuits the pipeline on failure so a DB transaction is never
/// opened for structurally invalid requests.
/// </remarks>
/// <typeparam name="TRequest">The MediatR request type being validated.</typeparam>
/// <typeparam name="TResponse">The response type returned by the handler.</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    /// <summary>
    /// Runs all validators in parallel. Throws <see cref="ValidationException"/> with
    /// every aggregated failure when at least one rule is violated; otherwise calls
    /// the next behavior / handler in the pipeline.
    /// </summary>
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        // Skip validation entirely when no validators are registered for this request type.
        if (!validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);

        // Run all validators concurrently then flatten every individual failure into one list.
        var failures = (await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, ct))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        // Throw a single exception that aggregates ALL rule violations so clients
        // receive the complete list of problems in one round-trip.
        if (failures.Count > 0) throw new ValidationException(failures);
        return await next();
    }
}