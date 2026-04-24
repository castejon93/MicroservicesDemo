using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Interfaces;
using MediatR;

namespace Auth.Application.Features.Auth.Login
{
    /// <summary>
    /// Handles <see cref="LoginCommand"/>: verifies credentials, rotates the refresh token,
    /// and returns a fresh token pair in an <see cref="AuthResponseDto"/>.
    /// </summary>
    public sealed class LoginCommandHandler(
        IUserRepository users,
        ITokenService tokens,
        IPasswordHasher hasher) : IRequestHandler<LoginCommand, AuthResponseDto>
    {
        /// <summary>
        /// Authenticates the user identified by <paramref name="request"/>.
        /// </summary>
        /// <param name="request">The login command carrying the email/username and password.</param>
        /// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
        /// <returns>
        /// An <see cref="AuthResponseDto"/> with <c>Success=true</c> and a fresh token pair on
        /// success, or <c>Success=false</c> with a generic error message on failure.
        /// </returns>
        public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await users.GetByEmailOrUsernameAsync(request.EmailOrUsername);

            // Return the SAME generic message for both "user not found" and "wrong password"
            // to prevent username enumeration (an attacker must not learn which field was wrong).
            if (user is null || !user.IsActive)
                return new AuthResponseDto { Success = false, Message = "Invalid credentials." };

            if (!hasher.Verify(request.Password, user.PasswordHash))
                return new AuthResponseDto { Success = false, Message = "Invalid credentials." };

            // Rotate the refresh token on every successful login to narrow the reuse window.
            var accessToken = tokens.GenerateAccessToken(user);
            var refreshToken = tokens.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = tokens.GetRefreshTokenExpiration();
            await users.UpdateAsync(user);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Login successful.",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
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
