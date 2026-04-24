using FluentValidation;

namespace Products.Application.Features.Products.DeleteProduct
{
    /// <summary>
    /// FluentValidation rules for <see cref="DeleteProductCommand"/>. Only validates
    /// that the identifier is positive; existence is checked by the handler so a
    /// missing product surfaces as a 404 rather than a 400.
    /// </summary>
    public sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
    {
        /// <summary>
        /// Configures validation rules for the delete-product command.
        /// </summary>
        public DeleteProductCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}