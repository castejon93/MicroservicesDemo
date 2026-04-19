// ============================================================
// FILE: src/Services/Auth/Auth.Domain/Interfaces/IUserRepository.cs
// PURPOSE: Repository interface for User persistence
// LAYER: Domain Layer (defines contract, not implementation)
// ============================================================

using Auth.Domain.Entities;

namespace Auth.Domain.Interfaces
{
    /// <summary>
    /// User Repository Interface
    /// 
    /// WHY interface in Domain layer?
    /// - Domain defines WHAT it needs (the contract)
    /// - Infrastructure provides HOW (the implementation)
    /// - This is Dependency Inversion Principle (DIP)
    /// 
    /// This allows:
    /// - Easy testing with mocks
    /// - Swapping database without changing domain
    /// - Clean separation of concerns
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Gets a user by their unique ID.
        /// </summary>
        Task<User?> GetByIdAsync(int id);

        /// <summary>
        /// Gets a user by their email address.
        /// </summary>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// Gets a user by their username.
        /// </summary>
        Task<User?> GetByUsernameAsync(string username);

        /// <summary>
        /// Gets a user by either email or username (for flexible login).
        /// </summary>
        Task<User?> GetByEmailOrUsernameAsync(string emailOrUsername);

        /// <summary>
        /// Checks if an email address is already registered.
        /// </summary>
        Task<bool> EmailExistsAsync(string email);

        /// <summary>
        /// Checks if a username is already taken.
        /// </summary>
        Task<bool> UsernameExistsAsync(string username);

        /// <summary>
        /// Adds a new user to the database.
        /// </summary>
        Task<User> AddAsync(User user);

        /// <summary>
        /// Updates an existing user's information.
        /// </summary>
        Task UpdateAsync(User user);
    }
}