namespace Auth.Application.Abstractions
{
    /// <summary>
    /// Transactional boundary abstraction owned by Application.
    /// Infrastructure implements this using EF Core's execution strategy so
    /// retries remain compatible with user-initiated transactions.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Runs <paramref name="operation"/> inside a transaction. If the configured
        /// execution strategy decides to retry (transient SQL failure), the ENTIRE
        /// delegate is re-invoked — make sure it's idempotent.
        /// </summary>
        Task<TResult> ExecuteInTransactionAsync<TResult>(
            Func<CancellationToken, Task<TResult>> operation,
            CancellationToken ct = default);
    }
}