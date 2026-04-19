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
        public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // Reject duplicates up front — cheaper than failing on the unique index.
            if (await users.EmailExistsAsync(request.Email))
                return new AuthResponseDto { Success = false, Message = "Email already exists." };

            if (await users.UsernameExistsAsync(request.Username))
                return new AuthResponseDto { Success = false, Message = "Username already taken." };

            var user = new User
            {
                Username = request.Username,
                Email = request.Email.ToLowerInvariant(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = hasher.Hash(request.Password),
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Issue tokens BEFORE persisting so we can store the refresh token in the same row.
            var accessToken = tokens.GenerateAccessToken(user);
            var refreshToken = tokens.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = tokens.GetRefreshTokenExpiration();

            var saved = await users.AddAsync(user);

            // Fire-and-forget domain event — subscribers handle side effects (welcome email, etc.).
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
