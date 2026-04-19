using Auth.Application.Cqrs;

namespace Auth.Application.Features.Auth.Logout
{
    /// <summary>
    /// Invalidates the server-side refresh token for the given user, preventing
    /// future /refresh calls from succeeding. Access tokens are stateless and
    /// simply expire on their own. Returns <c>true</c> when the user existed.
    /// </summary>
    public sealed record LogoutCommand(int UserId) : ICommand<bool>;
}
