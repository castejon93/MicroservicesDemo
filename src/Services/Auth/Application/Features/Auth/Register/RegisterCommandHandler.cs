using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Events;
using Auth.Domain.Interfaces;
using MediatR;

namespace Auth.Application.Features.Auth.Register
{
    /// <summary>
    /// Handles <see cref="RegisterCommand"/>: validates uniqueness, hashes the password,
    /// issues tokens, persists the user, and publishes a <see cref="UserRegistered"/> event.
    /// </summary>
    public sealed class RegisterCommandHandler(
        IUserRepository users,
        ITokenService tokens,
        IPasswordHasher hasher,
        IPublisher publisher) : IRequestHandler<RegisterCommand, AuthResponseDto>
    {
        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="request">The register command with all required user fields.</param>
        /// <param name="cancellationToken">Token to observe for cooperative cancellation.</param>
        /// <returns>
        /// An <see cref="AuthResponseDto"/> with <c>Success=true</c> and an issued token pair on
        /// success, or <c>Success=false</c> with a message when email or username is already taken.
        /// </returns>
        public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // Reject duplicates early — cheaper than waiting for a unique-constraint violation from SQL.
            if (await users.EmailExistsAsync(request.Email))
                return new AuthResponseDto { Success = false, Message = "Email already exists." };

            if (await users.UsernameExistsAsync(request.Username))
                return new AuthResponseDto { Success = false, Message = "Username already taken." };

            var user = new User
            {
                Username = request.Username,
                // Normalise email to lower-case for consistent uniqueness checks.
                Email = request.Email.ToLowerInvariant(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = hasher.Hash(request.Password),
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Generate tokens BEFORE persisting so the refresh token can be stored
            // on the same row in a single SaveChanges call.
            var accessToken = tokens.GenerateAccessToken(user);
            var refreshToken = tokens.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = tokens.GetRefreshTokenExpiration();

            var saved = await users.AddAsync(user);

            // Publish a domain event so interested handlers (e.g. welcome email) can
            // react without coupling directly to this handler.
            await publisher.Publish(new UserRegistered(saved.Id, saved.Email), cancellationToken);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Registration successful.",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = tokens.GetAccessTokenExpiration(),
                User = new UserInfo
                {
                    Id = saved.Id,
                    Username = saved.Username,
                    Email = saved.Email,
                    FirstName = saved.FirstName,
                    LastName = saved.LastName,
                    Role = saved.Role
                }
            };
        }
    }
}
