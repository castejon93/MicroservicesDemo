using MediatR;
using Microsoft.Extensions.Logging;

/// <summary>
/// MediatR pipeline behavior that logs the start and completion of every request,
/// including the elapsed wall-clock time in milliseconds.
/// </summary>
/// <remarks>
/// Registered first in the pipeline (<c>Program.cs</c>) so it wraps Validation and
/// Transaction behaviors, giving a true end-to-end duration for each request.
/// </remarks>
/// <typeparam name="TRequest">The MediatR request type being handled.</typeparam>
/// <typeparam name="TResponse">The response type returned by the handler.</typeparam>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<TRequest> logger)
    : IPipelineBehavior<TRequest, TResponse>
{
    /// <summary>
    /// Logs a start entry, invokes the rest of the pipeline, then logs the
    /// elapsed time in a <c>finally</c> block to guarantee the exit log even on exceptions.
    /// </summary>
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var name = typeof(TRequest).Name;

        // Log the request name before any processing begins.
        logger.LogInformation("▶ {Request}", name);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try { return await next(); }
        finally
        {
            // Always log completion time, even when the handler throws.
            logger.LogInformation("◀ {Request} ({Elapsed}ms)", name, sw.ElapsedMilliseconds);
        }
    }
}