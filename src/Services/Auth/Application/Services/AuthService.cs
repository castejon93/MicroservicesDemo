using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;

namespace Auth.Application.Services
{
    /// <summary>
    /// Application service that orchestrates authentication use cases.
    /// Lives in Application layer - coordinates domain and infrastructure.
    /// </summary>
    public class AuthService : IAuthService
    {
        // Domain layer dependencies (repository interfaces)
        private readonly IUserRepository _userRepository;

        // Infrastructure concerns abstracted through interfaces
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(
            IUserRepository userRepository,
            ITokenService tokenService,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            // Validation
            if (await _userRepository.EmailExistsAsync(request.Email))
            {
                return new AuthResponseDto { Success = false, Message = "Email already exists." };
            }

            if (await _userRepository.UsernameExistsAsync(request.Username))
            {
                return new AuthResponseDto { Success = false, Message = "Username already taken." };
            }

            // Create domain entity
            var user = new User
            {
                Username = request.Username,
                Email = request.Email.ToLower(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = _passwordHasher.Hash(request.Password),
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = _tokenService.GetRefreshTokenExpiration();

            // Persist
            await _userRepository.AddAsync(user);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Registration successful.",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = _tokenService.GetAccessTokenExpiration(),
                User = MapToUserInfo(user)
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _userRepository.GetByEmailOrUsernameAsync(request.EmailOrUsername);

            if (user == null || !user.IsActive)
            {
                return new AuthResponseDto { Success = false, Message = "Invalid credentials." };
            }

            if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                return new AuthResponseDto { Success = false, Message = "Invalid credentials." };
            }

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = _tokenService.GetRefreshTokenExpiration();
            await _userRepository.UpdateAsync(user);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Login successful.",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = _tokenService.GetAccessTokenExpiration(),
                User = MapToUserInfo(user)
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null)
            {
                return new AuthResponseDto { Success = false, Message = "Invalid access token." };
            }

            var userId = _tokenService.GetUserIdFromPrincipal(principal);
            if (userId == null)
            {
                return new AuthResponseDto { Success = false, Message = "Invalid token claims." };
            }

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null || user.RefreshToken != request.RefreshToken ||
                user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return new AuthResponseDto { Success = false, Message = "Invalid refresh token." };
            }

            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = _tokenService.GetRefreshTokenExpiration();
            await _userRepository.UpdateAsync(user);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Token refreshed.",
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = _tokenService.GetAccessTokenExpiration(),
                User = MapToUserInfo(user)
            };
        }

        public async Task<bool> RevokeTokenAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _userRepository.UpdateAsync(user);

            return true;
        }

        /// <summary>
        /// DEBUG: Gets a user by username for testing purposes.
        /// </summary>
        public async Task<UserDto?> GetUserByUsernameAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user is null)
            {
                return null;
            }

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                PasswordHash = user.PasswordHash,
                Role = user.Role,
                RefreshToken = user.RefreshToken,
                RefreshTokenExpiryTime = user.RefreshTokenExpiryTime,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        private static UserInfo MapToUserInfo(User user) => new()
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role
        };
    }
}
