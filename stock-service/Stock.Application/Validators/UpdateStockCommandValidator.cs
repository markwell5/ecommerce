using FluentValidation;
using Stock.Application.Commands;

namespace Stock.Application.Validators
{
    public class UpdateStockCommandValidator : AbstractValidator<UpdateStockCommand>
    {
        public UpdateStockCommandValidator()
        {
            RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0).WithMessage("Quantity must be non-negative");
            RuleFor(x => x.ProductId).GreaterThan(0);
        }
    }
}
