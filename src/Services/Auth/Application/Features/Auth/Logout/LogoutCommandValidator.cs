using FluentValidation;

namespace Auth.Application.Features.Auth.Logout
{
    // Ensures we always receive a positive UserId before touching the DB.
    public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
    {
        public LogoutCommandValidator()
        {
            RuleFor(x => x.UserId).GreaterThan(0);
        }
    }
}