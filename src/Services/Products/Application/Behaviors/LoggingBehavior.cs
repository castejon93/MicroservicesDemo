using MediatR;
using Microsoft.Extensions.Logging;

/// <summary>
/// Outermost MediatR pipeline behavior. Emits a structured log entry when a request
/// starts and another when it completes, including the elapsed milliseconds. Because
/// this behavior is registered first in <c>Program.cs</c>, its timing measurement
/// wraps the entire pipeline (validation + transaction + handler).
/// </summary>
/// <typeparam name="TRequest">The MediatR request type (command or query).</typeparam>
/// <typeparam name="TResponse">The response type returned by the handler.</typeparam>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<TRequest> logger)
    : IPipelineBehavior<TRequest, TResponse>
{
    /// <summary>
    /// Logs the request name on entry, delegates to the next component, and guarantees
    /// a completion log with elapsed time even if the inner pipeline throws.
    /// </summary>
    /// <param name="request">The incoming MediatR request.</param>
    /// <param name="next">Delegate that invokes the next behavior/handler.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    /// <returns>The response produced by the downstream handler.</returns>
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var name = typeof(TRequest).Name;
        logger.LogInformation("▶ {Request}", name);
        var sw = System.Diagnostics.Stopwatch.StartNew();
        // try/finally ensures the completion log is emitted even when the handler
        // throws (e.g. ValidationException or a domain exception).
        try { return await next(); }
        finally { logger.LogInformation("◀ {Request} ({Elapsed}ms)", name, sw.ElapsedMilliseconds); }
    }
}