using FluentValidation;

namespace Auth.Application.Features.Auth.Login
{
    /// <summary>
    /// Structural validation for <see cref="LoginCommand"/> — authorization checks
    /// (credential verification) happen in the handler.
    /// </summary>
    public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        /// <summary>
        /// Configures structural rules. Credential verification (password hash check)
        /// is intentionally left to <see cref="LoginCommandHandler"/> so that the
        /// validator stays purely structural and fast.
        /// </summary>
        public LoginCommandValidator()
        {
            // Both fields are required before we even touch the database.
            RuleFor(x => x.EmailOrUsername).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}
