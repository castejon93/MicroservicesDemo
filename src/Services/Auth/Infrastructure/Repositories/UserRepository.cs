using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories
{
    /// <summary>
    /// Implementation of IUserRepository using Entity Framework Core.
    /// Handles all database operations for User entities.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        // Database context injected via dependency injection
        private readonly AuthDbContext _context;

        /// <summary>
        /// Constructor - receives DbContext through DI.
        /// </summary>
        /// <param name="context">EF Core database context.</param>
        public UserRepository(AuthDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">The user's ID.</param>
        /// <returns>User if found, null otherwise.</returns>
        public async Task<User?> GetByIdAsync(int id)
        {
            // Use FindAsync for primary key lookups - it's optimized
            return await _context.Users.FindAsync(id);
        }

        /// <summary>
        /// Retrieves a user by their email address.
        /// Case-insensitive search.
        /// </summary>
        /// <param name="email">The email to search for.</param>
        /// <returns>User if found, null otherwise.</returns>
        public async Task<User?> GetByEmailAsync(string email)
        {
            // ToLower() ensures case-insensitive comparison
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        /// <summary>
        /// Retrieves a user by their username.
        /// Case-insensitive search.
        /// </summary>
        /// <param name="username">The username to search for.</param>
        /// <returns>User if found, null otherwise.</returns>
        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
        }

        /// <summary>
        /// Retrieves a user by either email or username.
        /// Enables flexible login (user can enter either).
        /// </summary>
        /// <param name="emailOrUsername">Email or username to search for.</param>
        /// <returns>User if found, null otherwise.</returns>
        public async Task<User?> GetByEmailOrUsernameAsync(string emailOrUsername)
        {
            // Normalize input for case-insensitive comparison
            var normalized = emailOrUsername.ToLower();

            return await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == normalized ||
                    u.Username.ToLower() == normalized);
        }

        /// <summary>
        /// Checks if an email address already exists in the database.
        /// Used during registration to prevent duplicates.
        /// </summary>
        /// <param name="email">Email to check.</param>
        /// <returns>True if email exists, false otherwise.</returns>
        public async Task<bool> EmailExistsAsync(string email)
        {
            // AnyAsync is more efficient than retrieving the full entity
            return await _context.Users
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        /// <summary>
        /// Checks if a username already exists in the database.
        /// Used during registration to prevent duplicates.
        /// </summary>
        /// <param name="username">Username to check.</param>
        /// <returns>True if username exists, false otherwise.</returns>
        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Users
                .AnyAsync(u => u.Username.ToLower() == username.ToLower());
        }

        /// <summary>
        /// Adds a new user to the database.
        /// </summary>
        /// <param name="user">The user entity to add.</param>
        /// <returns>The added user with generated ID.</returns>
        public async Task<User> AddAsync(User user)
        {
            // Add entity to change tracker
            await _context.Users.AddAsync(user);

            // Persist changes to database
            await _context.SaveChangesAsync();

            // Return user with generated ID
            return user;
        }

        /// <summary>
        /// Updates an existing user in the database.
        /// </summary>
        /// <param name="user">The user entity with updated values.</param>
        public async Task UpdateAsync(User user)
        {
            // Mark entity as modified
            _context.Users.Update(user);

            // Persist changes to database
            await _context.SaveChangesAsync();
        }
    }
}
