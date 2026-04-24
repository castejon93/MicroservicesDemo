using System;
using System.Collections.Generic;
using System.Text;

namespace Auth.Application.DTOs
{
    /// <summary>
    /// Full user data transfer object used internally within the Application layer.
    /// Contains all persisted user fields including security data.
    /// Do NOT expose this DTO directly to API consumers; use <see cref="UserInfo"/> instead.
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// Primary key - unique identifier for this user
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Unique username for login
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Unique email address for login and notifications
        /// </summary>
        public string Email { get; set; } = string.Empty;

        // ============================================
        // SECURITY
        // ============================================

        /// <summary>
        /// BCrypt hashed password - NEVER store plain text!
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// User role for authorization (Admin, User, etc.)
        /// </summary>
        public string Role { get; set; } = "User";

        /// <summary>
        /// Refresh token for JWT token renewal
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// When the refresh token expires
        /// </summary>
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // ============================================
        // PROFILE
        // ============================================

        /// <summary>User's first (given) name.</summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>User's last (family) name.</summary>
        public string LastName { get; set; } = string.Empty;

        // ============================================
        // AUDIT
        // ============================================

        /// <summary>UTC timestamp recording when the account was created.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>UTC timestamp of the last update; <see langword="null"/> if never modified.</summary>
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
