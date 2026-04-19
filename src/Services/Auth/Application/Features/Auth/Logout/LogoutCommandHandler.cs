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
        public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var user = await users.GetByIdAsync(request.UserId);
            if (user is null) return false;

            // Clear the server-side refresh token state.
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await users.UpdateAsync(user);
            return true;
        }
    }
}