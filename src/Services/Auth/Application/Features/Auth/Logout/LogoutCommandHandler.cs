using Auth.Domain.Interfaces;
using MediatR;

namespace Auth.Application.Features.Auth.Logout
{
    /// <summary>
    /// Invalidates the user's refresh token so subsequent /refresh calls are rejected.
    /// Access tokens are stateless and simply expire on their own.
    /// </summary>
    public sealed class LogoutCommandHandler(IUserRepository users)
        : IRequestHandler<LogoutCommand, bool>
    {
        /// <summary>
        /// Clears the server-side refresh token for the user identified by
        /// <paramref name="request"/>.<c>UserId</c>, preventing future token-refresh
        /// calls from succeeding. The stateless access token simply expires on its own.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the user was found and updated;
        /// <see langword="false"/> if the user does not exist.
        /// </returns>
        public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var user = await users.GetByIdAsync(request.UserId);
            if (user is null) return false;

            // Nullify both fields so any in-flight refresh token is immediately rejected.
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await users.UpdateAsync(user);
            return true;
        }
    }
}