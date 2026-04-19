using MediatR;

namespace Auth.Domain.Events
{
    /// <summary>
    /// Raised (via IPublisher) after a user successfully registers.
    /// Consumers can react (send welcome email, seed profile, enqueue audit, etc.)
    /// without coupling to the RegisterCommandHandler.
    /// </summary>
    public sealed record UserRegistered(int UserId, string Email) : INotification;
}
