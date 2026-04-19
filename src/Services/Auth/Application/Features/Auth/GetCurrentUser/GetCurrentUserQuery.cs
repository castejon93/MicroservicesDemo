using Auth.Application.Cqrs;
using Auth.Application.DTOs;

namespace Auth.Application.Features.Auth.GetCurrentUser
{
    // A query returns data but does NOT mutate state — it bypasses TransactionBehavior.
    public sealed record GetCurrentUserQuery(int UserId) : IQuery<UserInfo?>;
}