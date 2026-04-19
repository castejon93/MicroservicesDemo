using Auth.Application.Abstractions;
using Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Persistence
{
    /// <summary>
    /// EF Core-backed unit of work. Uses the DbContext's execution strategy so
    /// the transaction is retried as a single unit when the SqlServer retrying
    /// strategy decides to retry on a transient error.
    /// </summary>
    public sealed class EfUnitOfWork(AuthDbContext db) : IUnitOfWork
    {
        public async Task<TResult> ExecuteInTransactionAsync<TResult>(
            Func<CancellationToken, Task<TResult>> operation,
            CancellationToken ct = default)
        {
            // CreateExecutionStrategy() returns the strategy configured via
            // EnableRetryOnFailure(). Its ExecuteAsync wraps the delegate in
            // the retry loop; inside it we're allowed to open a transaction.
            var strategy = db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async cancellationToken =>
            {
                await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var result = await operation(cancellationToken);
                    await tx.CommitAsync(cancellationToken);
                    return result;
                }
                catch
                {
                    await tx.RollbackAsync(cancellationToken);
                    throw;
                }
            }, ct);
        }
    }
}