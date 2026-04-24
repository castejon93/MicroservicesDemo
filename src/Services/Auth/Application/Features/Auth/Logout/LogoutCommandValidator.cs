using FluentValidation;

namespace Auth.Application.Features.Auth.Logout
{
    /// <summary>
    /// Structural validation for <see cref="LogoutCommand"/>.
    /// Ensures a positive <c>UserId</c> is present before any database access occurs.
    /// </summary>
    public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
    {
        /// <summary>Configures the <c>UserId</c> must-be-positive rule.</summary>
        public LogoutCommandValidator()
        {
            RuleFor(x => x.UserId).GreaterThan(0);
        }
    }
}