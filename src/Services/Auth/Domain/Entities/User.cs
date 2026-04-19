// ============================================================
// FILE: src/Services/Auth/Auth.Domain/Entities/User.cs
// PURPOSE: User entity for authentication microservice
// LAYER: Domain Layer (innermost layer in Clean Architecture)
// ============================================================

namespace Auth.Domain.Entities
{
    /// <summary>
    /// User Entity - Core domain object for authentication
    /// 
    /// This entity is ISOLATED to the Auth microservice.
    /// Other microservices should NOT reference this directly.
    /// They communicate via:
    /// - JWT tokens (contains user claims)
    /// - API calls (if user info needed)
    /// - Message queues (for events like UserCreated)
    /// </summary>
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

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        // ============================================
        // AUDIT
        // ============================================

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}