using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Interfaces;
using MediatR;

namespace Auth.Application.Features.Auth.RefreshToken
{
    /// <summary>
    /// Handles <see cref="RefreshTokenCommand"/>: validates the (expired) access token
    /// + stored refresh token, rotates both, and returns the new pair.
    /// </summary>
    public sealed class RefreshTokenCommandHandler(
        IUserRepository users,
        ITokenService tokens) : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
    {
        /// <summary>
        /// Validates the (possibly expired) access token together with the stored refresh token
        /// and, when both are valid, rotates the token pair and persists the change.
        /// </summary>
        /// <param name="request">The command carrying the old access token and refresh token.</param>
        /// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
        /// <returns>
        /// An <see cref="AuthResponseDto"/> with a new token pair on success,
        /// or <c>Success=false</c> with a message when any validation step fails.
        /// </returns>
        public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // Validate signature/claims on the (likely expired) access token.
            // Lifetime validation is intentionally skipped — expiry is expected here.
            var principal = tokens.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal is null)
                return new AuthResponseDto { Success = false, Message = "Invalid access token." };

            var userId = tokens.GetUserIdFromPrincipal(principal);
            if (userId is null)
                return new AuthResponseDto { Success = false, Message = "Invalid token claims." };

            var user = await users.GetByIdAsync(userId.Value);

            // All three conditions must hold: user exists, refresh token matches stored value,
            // and the refresh token has not yet expired.
            if (user is null ||
                user.RefreshToken != request.RefreshToken ||
                user.RefreshTokenExpiryTime is null ||
                user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return new AuthResponseDto { Success = false, Message = "Invalid refresh token." };
            }

            // Rotate both tokens — the old refresh token is invalidated immediately.
            var newAccessToken = tokens.GenerateAccessToken(user);
            var newRefreshToken = tokens.GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = tokens.GetRefreshTokenExpiration();
            await users.UpdateAsync(user);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Token refreshed.",
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = tokens.GetAccessTokenExpiration(),
                User = new UserInfo
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role
                }
            };
        }
    }
}
