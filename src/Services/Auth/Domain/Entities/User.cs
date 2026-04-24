// ============================================================
// FILE: src/Services/Auth/Auth.Domain/Entities/User.cs
// PURPOSE: User entity for authentication microservice
// LAYER: Domain Layer (innermost layer in Clean Architecture)
// ============================================================

namespace Auth.Domain.Entities
{
    /// <summary>
    /// Core domain entity representing an authenticated user in the Auth microservice.
    /// </summary>
    /// <remarks>
    /// This entity is ISOLATED to the Auth microservice.
    /// Other microservices must NOT reference it directly; they communicate via:
    /// <list type="bullet">
    ///   <item><description>JWT tokens — user claims are embedded in the token payload.</description></item>
    ///   <item><description>API calls — if live user data is needed at runtime.</description></item>
    ///   <item><description>Domain events (e.g. <c>UserRegistered</c>) — for async side-effects.</description></item>
    /// </list>
    /// </remarks>
    public class User
    {
        // ============================================
        // IDENTITY
        // ============================================

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
        /// Opaque refresh token issued on login; used to obtain a new access token
        /// without re-entering credentials. Set to <see langword="null"/> on logout
        /// to invalidate the session server-side.
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// UTC expiry of the current <see cref="RefreshToken"/>.
        /// Null when no active refresh token is present (e.g. after logout).
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

        /// <summary>
        /// UTC timestamp of the most recent update to this record.
        /// <see langword="null"/> if the record has never been modified after creation.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Whether the account is currently active.
        /// Inactive accounts are rejected during login even with valid credentials.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}