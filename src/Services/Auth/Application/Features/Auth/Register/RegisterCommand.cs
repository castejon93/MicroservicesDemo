using Auth.Application.Cqrs;
using Auth.Application.DTOs;

namespace Auth.Application.Features.Auth.Register
{
    /// <summary>
    /// Write intent to register a new user account.
    /// Returns an <see cref="AuthResponseDto"/> with an issued token pair on success,
    /// or <c>Success=false</c> with a descriptive message when registration fails
    /// (e.g. duplicate email or username).
    /// </summary>
    /// <param name="Username">The desired unique login handle (3–50 characters).</param>
    /// <param name="Email">The user's email address; stored in lower-case and must be unique.</param>
    /// <param name="Password">The plain-text password; will be hashed before persistence.</param>
    /// <param name="FirstName">The user's first (given) name.</param>
    /// <param name="LastName">The user's last (family) name.</param>
    public sealed record RegisterCommand(
        string Username,
        string Email,
        string Password,
        string FirstName,
        string LastName) : ICommand<AuthResponseDto>;
}