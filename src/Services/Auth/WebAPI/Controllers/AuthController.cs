using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auth.WebAPI.Controllers
{
    /// <summary>
    /// API Controller for authentication operations.
    /// Handles user registration, login, token refresh, and logout.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        // Authentication service injected via DI
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// Constructor - receives dependencies through DI.
        /// </summary>
        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // ============================================
        // POST: api/auth/register
        // ============================================
        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="request">Registration details.</param>
        /// <returns>Auth response with tokens if successful.</returns>
        /// <response code="200">Registration successful, returns tokens.</response>
        /// <response code="400">Validation failed or email/username taken.</response>
        [HttpPost("register")]
        [AllowAnonymous] // No authentication required
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            // Log the registration attempt
            _logger.LogInformation("Registration attempt for email: {Email}", request.Email);

            // Call the auth service to register the user
            var result = await _authService.RegisterAsync(request);

            // Return appropriate status code based on result
            if (result.Success)
            {
                _logger.LogInformation("User registered successfully: {Email}", request.Email);
                return Ok(result);
            }

            _logger.LogWarning("Registration failed for email: {Email}. Reason: {Message}",
                request.Email, result.Message);
            return BadRequest(result);
        }

        // ============================================
        // POST: api/auth/login
        // ============================================
        /// <summary>
        /// Authenticates a user and returns tokens.
        /// </summary>
        /// <param name="request">Login credentials.</param>
        /// <returns>Auth response with tokens if credentials are valid.</returns>
        /// <response code="200">Login successful, returns tokens.</response>
        /// <response code="401">Invalid credentials.</response>
        [HttpPost("login")]
        [AllowAnonymous] // No authentication required
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            _logger.LogInformation("Login attempt for: {EmailOrUsername}", request.EmailOrUsername);

            var result = await _authService.LoginAsync(request);

            if (result.Success)
            {
                _logger.LogInformation("User logged in successfully: {EmailOrUsername}",
                    request.EmailOrUsername);
                return Ok(result);
            }

            _logger.LogWarning("Login failed for: {EmailOrUsername}. Reason: {Message}",
                request.EmailOrUsername, result.Message);
            return Unauthorized(result);
        }

        // ============================================
        // POST: api/auth/refresh-token
        // ============================================
        /// <summary>
        /// Refreshes an expired access token.
        /// </summary>
        /// <param name="request">The expired access token and refresh token.</param>
        /// <returns>Auth response with new tokens.</returns>
        /// <response code="200">Token refreshed successfully.</response>
        /// <response code="401">Invalid or expired refresh token.</response>
        [HttpPost("refresh-token")]
        [AllowAnonymous] // No authentication required (token is expired)
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            _logger.LogInformation("Token refresh attempt");

            var result = await _authService.RefreshTokenAsync(request);

            if (result.Success)
            {
                _logger.LogInformation("Token refreshed successfully for user: {UserId}",
                    result.User?.Id);
                return Ok(result);
            }

            _logger.LogWarning("Token refresh failed: {Message}", result.Message);
            return Unauthorized(result);
        }

        // ============================================
        // POST: api/auth/logout
        // ============================================
        /// <summary>
        /// Logs out the current user by revoking their refresh token.
        /// </summary>
        /// <returns>Success message.</returns>
        /// <response code="200">Logout successful.</response>
        /// <response code="401">User not authenticated.</response>
        [HttpPost("logout")]
        [Authorize] // Requires valid JWT token
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout()
        {
            // Get user ID from the JWT token claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                           ?? User.FindFirst("sub");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            _logger.LogInformation("Logout attempt for user ID: {UserId}", userId);

            var result = await _authService.RevokeTokenAsync(userId);

            if (result)
            {
                _logger.LogInformation("User logged out successfully: {UserId}", userId);
                return Ok(new { message = "Logged out successfully." });
            }

            return BadRequest(new { message = "Logout failed." });
        }

        // ============================================
        // GET: api/auth/me
        // ============================================
        /// <summary>
        /// Gets the current authenticated user's information.
        /// Useful for verifying the token and getting user details.
        /// </summary>
        /// <returns>Current user information.</returns>
        /// <response code="200">Returns user information.</response>
        /// <response code="401">User not authenticated.</response>
        [HttpGet("me")]
        [Authorize] // Requires valid JWT token
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetCurrentUser()
        {
            // Extract user information from JWT claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst("sub")?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value
                     ?? User.FindFirst("email")?.Value;
            var name = User.FindFirst(ClaimTypes.Name)?.Value
                    ?? User.FindFirst("name")?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new
            {
                id = userId,
                email = email,
                name = name,
                role = role
            });
        }

        // ============================================
        // DEBUG ENDPOINT - REMOVE IN PRODUCTION
        // ============================================
        /// <summary>
        /// DEBUG: Test password hash verification.
        /// Remove this endpoint before deploying to production!
        /// </summary>
        [HttpGet("debug/test-hash")]
        [AllowAnonymous]
        public async Task<IActionResult> DebugTestHash([FromQuery] string username, [FromQuery] string password)
        {
            var user = await _authService.GetUserByUsernameAsync(username);

            if (user == null)
            {
                return Ok(new
                {
                    userFound = false,
                    message = $"User '{username}' not found in database"
                });
            }

            // Test password verification
            var storedHash = user.PasswordHash;
            var isValid = BCrypt.Net.BCrypt.Verify(password, storedHash);

            // Generate a new hash to compare
            var newHash = BCrypt.Net.BCrypt.HashPassword(password, 12);

            return Ok(new
            {
                userFound = true,
                username = user.Username,
                email = user.Email,
                isActive = user.IsActive,
                storedHashPrefix = storedHash.Substring(0, 29), // Show work factor and salt only
                storedHashLength = storedHash.Length,
                passwordProvided = password,
                verificationResult = isValid,
                newHashForPassword = newHash,
                message = isValid ? "Password matches!" : "Password does NOT match"
            });
        }
    }
}
