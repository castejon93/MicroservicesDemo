namespace Auth.Application.Interfaces
{
    /// <summary>
    /// Abstraction for password hashing and verification.
    /// Defined in Application; implemented in Infrastructure so the domain
    /// stays free of any third-party hashing library dependency.
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Produces a one-way hash of <paramref name="password"/> safe for persistent storage.
        /// The implementation automatically generates and embeds a cryptographic salt so
        /// each call returns a unique hash even for the same plain-text input.
        /// </summary>
        /// <param name="password">The plain-text password to hash. Must not be <see langword="null"/> or empty.</param>
        /// <returns>
        /// An encoded string containing the algorithm identifier, work factor, salt, and hash
        /// (e.g. a BCrypt-format string). Safe to store directly in the database.
        /// </returns>
        string Hash(string password);

        /// <summary>
        /// Verifies that <paramref name="password"/> matches the previously computed
        /// <paramref name="hashedPassword"/>.
        /// The salt and work factor are extracted from <paramref name="hashedPassword"/> internally;
        /// callers do not need to manage them separately.
        /// </summary>
        /// <param name="password">The plain-text password supplied by the user at login time.</param>
        /// <param name="hashedPassword">The stored hash to compare against (from <see cref="Hash"/>).</param>
        /// <returns><see langword="true"/> if the password is correct; otherwise <see langword="false"/>.</returns>
        bool Verify(string password, string hashedPassword);
    }
}
