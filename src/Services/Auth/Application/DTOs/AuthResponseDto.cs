namespace Auth.Application.DTOs
{
    /// <summary>
    /// Response returned after successful authentication.
    /// Contains tokens and user information.
    /// </summary>
    public class AuthResponseDto
    {
        /// <summary>
        /// Indicates if the authentication was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Message describing the result (success or error details).
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// JWT access token - include in Authorization header for API calls.
        /// Format: "Bearer {AccessToken}"
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Refresh token - use to obtain new access token when it expires.
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// When the access token expires (UTC).
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Basic user information returned after authentication.
        /// </summary>
        public UserInfo? User { get; set; }
    }

    /// <summary>
    /// Minimal user projection returned as part of an <see cref="AuthResponseDto"/>.
    /// Contains only the fields needed by API clients; sensitive data such as
    /// the password hash and refresh token are intentionally omitted.
    /// </summary>
    public class UserInfo
    {
        /// <summary>The user's unique database identifier.</summary>
        public int Id { get; set; }

        /// <summary>The user's unique login handle.</summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>The user's email address.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>The user's first (given) name.</summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>The user's last (family) name.</summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>The user's authorization role (e.g. <c>"Admin"</c>, <c>"User"</c>).</summary>
        public string Role { get; set; } = string.Empty;
    }
}