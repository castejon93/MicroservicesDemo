using Auth.Application.Cqrs;
using Auth.Application.DTOs;

namespace Auth.Application.Features.Auth.Login
{
    /// <summary>
    /// Write intent to authenticate a user by email OR username + password.
    /// Returns an <see cref="AuthResponseDto"/> containing freshly issued access &amp;
    /// refresh tokens on success; <c>Success=false</c> with a descriptive <c>Message</c> on failure.
    /// </summary>
    /// <param name="EmailOrUsername">The user's email address or username (either is accepted).</param>
    /// <param name="Password">The plain-text password to verify against the stored hash.</param>
    public sealed record LoginCommand(
        string EmailOrUsername,
        string Password) : ICommand<AuthResponseDto>;
}
