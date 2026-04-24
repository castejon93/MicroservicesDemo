using FluentValidation;

namespace Products.Application.Features.Products.UpdateProduct
{
    /// <summary>
    /// FluentValidation rules for <see cref="UpdateProductCommand"/>. Mirrors the
    /// create-product rules but additionally requires a positive identifier since
    /// update targets an existing aggregate.
    /// </summary>
    public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        /// <summary>
        /// Configures validation rules for the update-product command.
        /// </summary>
        public UpdateProductCommandValidator()
        {
            // Id comes from the route; guard against 0/negative values that could never match a row.
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
        }
    }
}