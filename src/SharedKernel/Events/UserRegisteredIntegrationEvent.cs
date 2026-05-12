namespace SharedKernel.Events;

/// <summary>
/// Published by the <c>Auth</c> service when a new user completes registration.
/// Consumers (e.g. Products, Notifications) subscribe to this event to perform
/// their own reactions without coupling to the Auth service directly.
/// </summary>
/// <param name="UserId">Database PK of the newly created user.</param>
/// <param name="Username">Chosen username; useful for display in welcome emails etc.</param>
/// <param name="Email">Verified email address of the new user.</param>
/// <param name="Role">Role assigned at registration (e.g. "User", "Admin").</param>
public sealed record UserRegisteredIntegrationEvent(
    int UserId,
    string Username,
    string Email,
    string Role) : IntegrationEvent;