using Auth.Application.Cqrs;
using Auth.Application.DTOs;

namespace Auth.Application.Features.Auth.GetCurrentUser
{
    /// <summary>
    /// Read-only request that returns the <see cref="UserInfo"/> projection for a given user ID.
    /// As a query it does not mutate state and therefore bypasses <c>TransactionBehavior</c>.
    /// </summary>
    /// <param name="UserId">The database ID of the user whose profile is being retrieved.</param>
    public sealed record GetCurrentUserQuery(int UserId) : IQuery<UserInfo?>;
}