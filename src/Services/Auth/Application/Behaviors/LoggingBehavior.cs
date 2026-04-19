using MediatR;
using Microsoft.Extensions.Logging;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<TRequest> logger)
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var name = typeof(TRequest).Name;
        logger.LogInformation("▶ {Request}", name);
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try { return await next(); }
        finally { logger.LogInformation("◀ {Request} ({Elapsed}ms)", name, sw.ElapsedMilliseconds); }
    }
}