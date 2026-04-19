using FluentValidation;

namespace Auth.Application.Features.Auth.Login
{
    /// <summary>
    /// Structural validation for <see cref="LoginCommand"/> — authorization checks
    /// (credential verification) happen in the handler.
    /// </summary>
    public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.EmailOrUsername).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}
