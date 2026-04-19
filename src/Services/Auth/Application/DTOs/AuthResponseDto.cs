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
    /// Basic user information included in auth response.
    /// Does not include sensitive data like password hash.
    /// </summary>
    public class UserInfo
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}