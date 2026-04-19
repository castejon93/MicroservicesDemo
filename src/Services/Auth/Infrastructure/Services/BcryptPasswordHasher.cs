using Auth.Application.Interfaces;

namespace Auth.Infrastructure.Services
{
    /// <summary>
    /// BCrypt implementation of IPasswordHasher.
    /// 
    /// BCrypt is a secure password hashing algorithm that:
    /// - Automatically generates and stores salt
    /// - Uses adaptive cost factor (work factor)
    /// - Is resistant to rainbow table attacks
    /// - Is designed to be slow (prevents brute force attacks)
    /// </summary>
    public class BcryptPasswordHasher : IPasswordHasher
    {
        // Work factor: higher = more secure but slower
        // 12 is a good balance (about 250ms per hash)
        private const int WorkFactor = 12;

        /// <summary>
        /// Hashes a plain text password using BCrypt.
        /// Salt is automatically generated and embedded in the hash.
        /// </summary>
        /// <param name="password">The plain text password to hash</param>
        /// <returns>The BCrypt hash (includes salt and work factor)</returns>
        public string Hash(string password)
        {
            return global::BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        /// <summary>
        /// Verifies a password against a stored BCrypt hash.
        /// </summary>
        /// <param name="password">The plain text password to verify</param>
        /// <param name="hashedPassword">The stored BCrypt hash</param>
        /// <returns>True if the password matches, false otherwise</returns>
        public bool Verify(string password, string hashedPassword)
        {
            return global::BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
